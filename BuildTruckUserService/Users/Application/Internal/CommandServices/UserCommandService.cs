using BuildTruckUserService.Users.Domain.Model.Aggregates;
using BuildTruckUserService.Users.Domain.Model.Commands;
using BuildTruckUserService.Users.Domain.Model.ValueObjects;
using BuildTruckUserService.Users.Domain.Repositories;
using BuildTruckUserService.Users.Domain.Services;
using BuildTruckShared.Domain.Repositories;
using BuildTruckUserService.Users.Application.ACL.Services;
using BuildTruckShared.Infrastructure.ExternalServices.Email.Services;

namespace BuildTruckUserService.Users.Application.Internal.CommandServices;

public class UserCommandService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IEmailService userEmailService,
    IGenericEmailService genericEmailService,
    IImageService imageService)
    : IUserCommandService
{
    public async Task<User> Handle(CreateUserCommand command)
    {
        var personName = new PersonName(command.Name, command.LastName);
        var corporateEmail = EmailAddress.GenerateCorporateEmail(personName);

        if (await userRepository.ExistsByEmailAsync(corporateEmail))
            throw new InvalidOperationException($"Email {corporateEmail.Address} already exists");

        var temporalPassword = GenerateTemporalPassword();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(temporalPassword);

        var user = new User(command, passwordHash);

        await userRepository.AddAsync(user);
        await unitOfWork.CompleteAsync();

        try
        {
            await userEmailService.SendUserCredentialsAsync(user, temporalPassword);
        }
        catch (Exception emailEx)
        {
            Console.WriteLine($"Email error (non-fatal): {emailEx.Message}");
        }

        return user;
    }

    public async Task<User> Handle(ChangePasswordCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.UserId)
            ?? throw new ArgumentException($"User with ID {command.UserId} not found");

        if (!BCrypt.Net.BCrypt.Verify(command.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect");

        ValidatePassword(command.NewPassword);

        user.UpdatePasswordHash(BCrypt.Net.BCrypt.HashPassword(command.NewPassword));
        userRepository.Update(user);
        await unitOfWork.CompleteAsync();

        try
        {
            await genericEmailService.SendPasswordChangedNotificationAsync(user.Email, user.FullName);
        }
        catch (Exception emailEx)
        {
            Console.WriteLine($"Email error (non-fatal): {emailEx.Message}");
        }

        return user;
    }

    public async Task Handle(DeleteUserCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.UserId)
            ?? throw new ArgumentException($"User with ID {command.UserId} not found");

        userRepository.Remove(user);
        await unitOfWork.CompleteAsync();
    }

    public async Task<User> Handle(UploadProfileImageCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.UserId)
            ?? throw new ArgumentException($"User with ID {command.UserId} not found");

        var validation = imageService.ValidateUserProfileImage(command.ImageBytes, command.FileName);
        if (!validation.IsValid)
            throw new ArgumentException(validation.ErrorMessage);

        var imageUrl = await imageService.UploadUserProfileImageAsync(user, command.ImageBytes, command.FileName);
        user.UpdateProfileImage(imageUrl);

        userRepository.Update(user);
        await unitOfWork.CompleteAsync();

        return user;
    }

    public async Task<User> Handle(DeleteProfileImageCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.UserId)
            ?? throw new ArgumentException($"User with ID {command.UserId} not found");

        var deleted = await imageService.DeleteUserProfileImageAsync(user);
        if (deleted)
        {
            user.UpdateProfileImage(null);
            userRepository.Update(user);
            await unitOfWork.CompleteAsync();
        }

        return user;
    }

    public async Task<User> Handle(UpdateUserCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.UserId)
            ?? throw new ArgumentException($"User with ID {command.UserId} not found");

        if (!command.HasChanges)
            throw new ArgumentException("No changes provided");

        if (command.WillUpdateName)
        {
            var tempName = new PersonName(
                command.Name ?? user.Name.FirstName,
                command.LastName ?? user.Name.LastName);
            var newEmail = EmailAddress.GenerateCorporateEmail(tempName);

            if (newEmail.Address != user.Email && await userRepository.ExistsByEmailAsync(newEmail))
                throw new InvalidOperationException($"Email {newEmail.Address} already exists");
        }

        user.UpdateInfo(
            firstName: command.Name,
            lastName: command.LastName,
            personalEmail: command.PersonalEmail,
            phone: null,
            role: command.Role);

        userRepository.Update(user);
        await unitOfWork.CompleteAsync();

        return user;
    }

    private static string GenerateTemporalPassword() => $"Temp{Random.Shared.Next(1000, 9999)}!";

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters");
        if (!password.Any(char.IsUpper))
            throw new ArgumentException("Password must contain at least one uppercase letter");
        if (!password.Any(char.IsLower))
            throw new ArgumentException("Password must contain at least one lowercase letter");
        if (!password.Any(char.IsDigit))
            throw new ArgumentException("Password must contain at least one number");
        if (!password.Any(ch => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(ch)))
            throw new ArgumentException("Password must contain at least one special character");
    }
}
