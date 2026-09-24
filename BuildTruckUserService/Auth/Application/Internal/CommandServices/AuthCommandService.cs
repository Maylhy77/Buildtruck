using BuildTruckUserService.Auth.Application.ACL.Services;
using BuildTruckUserService.Auth.Domain.Model.Commands;
using BuildTruckUserService.Auth.Domain.Model.ValueObjects;
using BuildTruckUserService.Auth.Domain.Services;
using BuildTruckUserService.Auth.Infrastructure.Tokens.JWT.Services;
using Microsoft.Extensions.Logging;

namespace BuildTruckUserService.Auth.Application.Internal.CommandServices;

/// <summary>
/// Auth Command Service Implementation
/// </summary>
/// <remarks>
/// Application service that orchestrates authentication commands
/// </remarks>
public class AuthCommandService : IAuthCommandService
{
    private readonly IUserContextService _userContextService;
    private readonly TokenService _tokenService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuthCommandService> _logger;

    public AuthCommandService(
        IUserContextService userContextService,
        TokenService tokenService,
        IServiceProvider serviceProvider,
        ILogger<AuthCommandService> logger)
    {
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AuthToken?> HandleSignInAsync(SignInCommand command)
    {
        try
        {
            _logger.LogInformation("Processing sign-in command: {AuditInfo}", command.GetAuditString());

            // Validate command
            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid sign-in command for email: {Email}", command.Email);
                return null;
            }

            // Authenticate user via ACL
            var authenticatedUser = await _userContextService.AuthenticateUserAsync(
                command.Email, 
                command.Password);

            if (authenticatedUser == null)
            {
                _logger.LogWarning("Authentication failed for email: {Email}", command.Email);
                return null;
            }

            _logger.LogInformation("User authenticated successfully: {UserId} - {Email}", 
                authenticatedUser.Id, authenticatedUser.Email);

            // Generate JWT token
            var authToken = _tokenService.GenerateToken(authenticatedUser);

            // Update last login timestamp in background with proper scope
            _ = Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var scopedUserService = scope.ServiceProvider.GetRequiredService<IUserContextService>();
                try
                {
                    await scopedUserService.UpdateUserLastLoginAsync(authenticatedUser.Id);
                    _logger.LogDebug("Last login updated for user: {UserId}", authenticatedUser.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update last login for user: {UserId}", authenticatedUser.Id);
                }
            });

            _logger.LogInformation("Sign-in completed successfully for user: {UserId}", authenticatedUser.Id);
            return authToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sign-in command for email: {Email}", command.Email);
            return null;
        }
    }
    // Agregar estos métodos a tu AuthCommandService.cs existente

public async Task<bool> HandleSendPasswordResetAsync(SendPasswordResetCommand command)
{
    try
    {
        _logger.LogInformation("Processing send password reset command: {AuditInfo}", command.GetAuditString());

        // Validate command
        if (!command.IsValid())
        {
            _logger.LogWarning("Invalid send password reset command for email: {Email}", command.Email);
            return false;
        }

        // Check if user exists via ACL
        var user = await _userContextService.GetUserByEmailAsync(command.Email);
        if (user == null)
        {
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", command.Email);
            // ⚠️ IMPORTANTE: Siempre retornar true por seguridad (no revelar si el email existe)
            return true;
        }

        _logger.LogInformation("User found for password reset: {UserId} - {Email}", user.Id, user.Email);

        // Generate password reset token
        var resetToken = _tokenService.GeneratePasswordResetToken(user.Id, user.Email);

        // Send reset email via ACL
        await _userContextService.SendPasswordResetEmailAsync(user, resetToken);

        _logger.LogInformation("Password reset email sent successfully for user: {UserId}", user.Id);
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing send password reset command for email: {Email}", command.Email);
        // ⚠️ Retornar true para no revelar errores internos al usuario
        return true;
    }
}

public async Task<bool> HandleResetPasswordAsync(ResetPasswordCommand command)
{
    try
    {
        _logger.LogInformation("Processing reset password command: {AuditInfo}", command.GetAuditString());

        // Validate command
        if (!command.IsValid())
        {
            _logger.LogWarning("Invalid reset password command for email: {Email}", command.Email);
            return false;
        }

        // Validate password complexity
        if (!command.IsPasswordComplex())
        {
            _logger.LogWarning("Password does not meet complexity requirements for email: {Email}", command.Email);
            return false;
        }

        // Validate reset token
        var (isValid, userId, tokenEmail) = _tokenService.ValidatePasswordResetToken(command.Token);
        if (!isValid)
        {
            _logger.LogWarning("Invalid or expired reset token for email: {Email}", command.Email);
            return false;
        }

        // Verify email matches token
        if (!string.Equals(command.Email, tokenEmail, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Email mismatch in reset password: requested={RequestedEmail}, token={TokenEmail}", 
                command.Email, tokenEmail);
            return false;
        }

        // Get user to verify they still exist and are active
        var user = await _userContextService.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found for password reset: {UserId}", userId);
            return false;
        }

        _logger.LogInformation("Resetting password for user: {UserId} - {Email}", user.Id, user.Email);

        // Reset password via ACL
        var success = await _userContextService.ResetUserPasswordAsync(user.Id, command.NewPassword);
        if (!success)
        {
            _logger.LogError("Failed to reset password for user: {UserId}", user.Id);
            return false;
        }

        _logger.LogInformation("Password reset completed successfully for user: {UserId}", user.Id);
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing reset password command for email: {Email}", command.Email);
        return false;
    }
}
}