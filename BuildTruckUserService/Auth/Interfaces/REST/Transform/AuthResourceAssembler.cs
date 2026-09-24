using BuildTruckUserService.Auth.Domain.Model.Commands;
using BuildTruckUserService.Auth.Domain.Model.ValueObjects;
using BuildTruckUserService.Auth.Interfaces.REST.Resources;

namespace BuildTruckUserService.Auth.Interfaces.REST.Transform;

/// <summary>
/// Auth Resource Assembler
/// </summary>
/// <remarks>
/// Transforms between API resources and domain objects
/// </remarks>
public static class AuthResourceAssembler
{
    /// <summary>
    /// Transform SignInResource to SignInCommand
    /// </summary>
    /// <param name="resource">Sign-in resource from API</param>
    /// <param name="ipAddress">Client IP address</param>
    /// <param name="userAgent">Client user agent</param>
    /// <returns>SignInCommand for domain processing</returns>
    public static SignInCommand ToCommand(SignInResource resource, string? ipAddress = null, string? userAgent = null)
    {
        return new SignInCommand(
            resource.Email,
            resource.Password,
            ipAddress,
            userAgent);
    }

    /// <summary>
    /// Transform AuthenticatedUser to AuthenticatedUserResource
    /// </summary>
    /// <param name="user">Authenticated user from domain</param>
    /// <returns>AuthenticatedUserResource for API response</returns>
    public static AuthenticatedUserResource ToResource(AuthenticatedUser user)
    {
        return new AuthenticatedUserResource
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            ProfileImageUrl = user.ProfileImageUrl,
            LastLogin = user.LastLogin,
            RoleDisplay = user.GetDisplayRole(),
            Capabilities = new UserCapabilitiesResource
            {
                CanManageProjects = user.CanManageProjects(),
                CanBeAssignedToProject = user.CanBeAssignedToProject(),
                IsAdmin = user.IsAdmin(),
                IsManager = user.IsManager(),
                IsSupervisor = user.IsSupervisor(),
                IsWorker = user.IsWorker()
            }
        };
    }

    /// <summary>
    /// Transform AuthToken and AuthenticatedUser to complete sign-in response
    /// </summary>
    /// <param name="authToken">JWT token from domain</param>
    /// <returns>Complete sign-in response with user and token</returns>
    public static object ToSignInResponse(AuthToken authToken)
    {
        // Extract user info from token claims
        var userId = authToken.GetUserId();
        var fullName = authToken.GetClaimValue("full_name");
        var email = authToken.GetUserEmail();
        var role = authToken.GetUserRole();
        var profileImage = authToken.GetClaimValue("profile_image");
        var lastLoginStr = authToken.GetClaimValue("last_login");
        
        DateTime? lastLogin = null;
        if (!string.IsNullOrEmpty(lastLoginStr) && DateTime.TryParse(lastLoginStr, out var parsedLastLogin))
        {
            lastLogin = parsedLastLogin;
        }

        // Create AuthenticatedUser from token claims
        var user = new AuthenticatedUser(userId, fullName, email, role, profileImage, lastLogin);

        return new
        {
            user = ToResource(user),
            token = authToken.Token,
            expiresAt = authToken.ExpiresAt,
            tokenType = authToken.TokenType
        };
    }

    /// <summary>
    /// Create error response for authentication failure
    /// </summary>
    /// <param name="message">Error message</param>
    /// <returns>Error response object</returns>
    public static object ToErrorResponse(string message = "Invalid credentials")
    {
        return new
        {
            error = true,
            message = message,
            timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create unauthorized response
    /// </summary>
    /// <param name="message">Error message</param>
    /// <returns>Unauthorized response object</returns>
    public static object ToUnauthorizedResponse(string message = "Unauthorized")
    {
        return new
        {
            error = true,
            message = message,
            timestamp = DateTime.UtcNow
        };
    }
}