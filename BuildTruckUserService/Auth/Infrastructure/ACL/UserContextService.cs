using BuildTruckUserService.Auth.Application.ACL.Services;
using BuildTruckUserService.Auth.Domain.Model.ValueObjects;
using BuildTruckUserService.Users.Domain.Model.Aggregates;
using BuildTruckUserService.Users.Application.Internal.OutboundServices;
using Microsoft.Extensions.Logging;

namespace BuildTruckUserService.Auth.Infrastructure.ACL;

/// <summary>
/// ACL Service implementation for communication with Users Context
/// </summary>
/// <remarks>
/// Translates between Auth Context and Users Context, converting Users.User to Auth.AuthenticatedUser
/// </remarks>
public class UserContextService : IUserContextService
{
    private readonly IUserFacade _userFacade;
    private readonly ILogger<UserContextService> _logger;

    public UserContextService(IUserFacade userFacade, ILogger<UserContextService> logger)
    {
        _userFacade = userFacade ?? throw new ArgumentNullException(nameof(userFacade));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AuthenticatedUser?> AuthenticateUserAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("Attempting to authenticate user with email: {Email}", email);
            
            // Verificar credenciales usando Users Context
            var user = await _userFacade.VerifyCredentialsAsync(email, password);
            
            if (user == null)
            {
                _logger.LogWarning("Authentication failed for email: {Email}", email);
                return null;
            }

            // Verificar que el usuario esté activo
            if (!user.IsActive)
            {
                _logger.LogWarning("Authentication failed - user is inactive: {Email}", email);
                return null;
            }

            _logger.LogInformation("Authentication successful for user: {UserId} - {Email}", user.Id, email);
            
            // Convertir Users.User a Auth.AuthenticatedUser
            return new AuthenticatedUser(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for email: {Email}", email);
            return null;
        }
    }

    public async Task<AuthenticatedUser?> GetUserByIdAsync(int userId)
    {
        try
        {
            _logger.LogDebug("Getting user by ID: {UserId}", userId);
            
            var user = await _userFacade.FindByIdAsync(userId);
            
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                return null;
            }

            // Verificar que el usuario esté activo
            if (!user.IsActive)
            {
                _logger.LogWarning("User found but is inactive: {UserId}", userId);
                return null;
            }

            _logger.LogDebug("User found: {UserId} - {Email}", user.Id, user.Email);
            
            // Convertir Users.User a Auth.AuthenticatedUser
            return new AuthenticatedUser(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> UpdateUserLastLoginAsync(int userId)
    {
        try
        {
            _logger.LogDebug("Updating last login for user: {UserId}", userId);
            
            var result = await _userFacade.UpdateLastLoginAsync(userId);
            
            if (result)
            {
                _logger.LogDebug("Last login updated successfully for user: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("Failed to update last login for user: {UserId}", userId);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsUserActiveAsync(string email)
    {
        try
        {
            _logger.LogDebug("Checking if user is active: {Email}", email);
            
            var isActive = await _userFacade.IsActiveUserAsync(email);
            
            _logger.LogDebug("User active status for {Email}: {IsActive}", email, isActive);
            
            return isActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user active status for email: {Email}", email);
            return false;
        }
    }

    public async Task<string?> GetUserProfileImageUrlAsync(int userId, int size = 200)
    {
        try
        {
            _logger.LogDebug("Getting profile image URL for user: {UserId}, size: {Size}", userId, size);
            
            var imageUrl = await _userFacade.GetUserProfileImageUrlAsync(userId, size);
            
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                _logger.LogDebug("Profile image URL found for user: {UserId}", userId);
            }
            else
            {
                _logger.LogDebug("No profile image URL found for user: {UserId}", userId);
            }
            
            return imageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile image URL for user: {UserId}", userId);
            return null;
        }
    }
    
    // Agregar estos métodos a tu UserContextService.cs existente

public async Task<AuthenticatedUser?> GetUserByEmailAsync(string email)
{
    try
    {
        _logger.LogDebug("Getting user by email: {Email}", email);
        
        var user = await _userFacade.FindByEmailAsync(email);
        
        if (user == null)
        {
            _logger.LogWarning("User not found with email: {Email}", email);
            return null;
        }

        // Verificar que el usuario esté activo
        if (!user.IsActive)
        {
            _logger.LogWarning("User found but is inactive: {Email}", email);
            return null;
        }

        _logger.LogDebug("User found: {UserId} - {Email}", user.Id, user.Email);
        
        // Convertir Users.User a Auth.AuthenticatedUser
        return new AuthenticatedUser(user);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting user by email: {Email}", email);
        return null;
    }
}

    public async Task SendPasswordResetEmailAsync(AuthenticatedUser user, string resetToken)
    {
        try
        {
            _logger.LogInformation("Sending password reset email for user: {UserId} - {Email}", user.Id, user.Email);
            
            // Usar el facade para enviar el email
            await _userFacade.SendPasswordResetEmailAsync(user.Id, user.Email, user.FullName, resetToken);
            
            _logger.LogInformation("Password reset email sent successfully for user: {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email for user: {UserId}", user.Id);
            throw; // Re-throw para que el caller pueda manejar el error
        }
    }

    public async Task<bool> ResetUserPasswordAsync(int userId, string newPassword)
    {
        try
        {
            _logger.LogInformation("Resetting password for user: {UserId}", userId);
            
            var result = await _userFacade.ResetUserPasswordAsync(userId, newPassword);
            
            if (result)
            {
                _logger.LogInformation("Password reset successfully for user: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("Failed to reset password for user: {UserId}", userId);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user: {UserId}", userId);
            return false;
        }
    }
}