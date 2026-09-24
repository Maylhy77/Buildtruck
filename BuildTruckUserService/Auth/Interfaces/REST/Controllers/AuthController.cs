using System.Security.Claims;
using BuildTruckUserService.Auth.Domain.Model.Commands;
using BuildTruckUserService.Auth.Domain.Model.Queries;
using BuildTruckUserService.Auth.Domain.Services;
using BuildTruckUserService.Auth.Interfaces.REST.Resources;
using BuildTruckUserService.Auth.Interfaces.REST.Transform;
using BuildTruckUserService.Users.Application.Internal.OutboundServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildTruckUserService.Auth.Interfaces.REST.Controllers;

/// <summary>
/// Authentication Controller
/// </summary>
/// <remarks>
/// Handles authentication endpoints for the Auth bounded context
/// </remarks>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthCommandService _authCommandService;
    private readonly IAuthQueryService _authQueryService;
    private readonly IUserFacade _userFacade;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthCommandService authCommandService,
        IAuthQueryService authQueryService,
        IUserFacade userFacade,
        ILogger<AuthController> logger)
    {
        _authCommandService = authCommandService ?? throw new ArgumentNullException(nameof(authCommandService));
        _authQueryService = authQueryService ?? throw new ArgumentNullException(nameof(authQueryService));
        _userFacade = userFacade ?? throw new ArgumentNullException(nameof(userFacade));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// User sign-in
    /// </summary>
    /// <param name="resource">Sign-in credentials</param>
    /// <returns>Authentication token and user information</returns>
    /// <response code="200">Authentication successful</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SignIn([FromBody] SignInResource resource)
    {
        try
        {
            _logger.LogInformation("Sign-in attempt for email: {Email}", resource.Email);

            // Validate model state
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid sign-in request for email: {Email}", resource.Email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            // Get client information
            var ipAddress = GetClientIpAddress();
            var userAgent = GetClientUserAgent();

            // Transform to domain command
            var command = AuthResourceAssembler.ToCommand(resource, ipAddress, userAgent);

            // Execute sign-in
            var authToken = await _authCommandService.HandleSignInAsync(command);

            if (authToken == null)
            {
                _logger.LogWarning("Sign-in failed for email: {Email}", resource.Email);
                return Unauthorized(AuthResourceAssembler.ToErrorResponse("Invalid credentials"));
            }

            _logger.LogInformation("Sign-in successful for user: {UserId}", authToken.GetUserId());

            // Transform to response
            var response = AuthResourceAssembler.ToSignInResponse(authToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign-in for email: {Email}", resource.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                AuthResourceAssembler.ToErrorResponse("An error occurred during authentication"));
        }
    }

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    /// <returns>Current user information</returns>
    /// <response code="200">User information retrieved successfully</response>
    /// <response code="401">Not authenticated or token expired</response>
    /// <response code="404">User not found or inactive</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AuthenticatedUserResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            // Get user ID from JWT claims
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                _logger.LogWarning("Invalid or missing user ID in JWT claims");
                return Unauthorized(AuthResourceAssembler.ToUnauthorizedResponse("Invalid token"));
            }

            _logger.LogDebug("Getting current user information for user: {UserId}", userId);

            // Get client information
            var ipAddress = GetClientIpAddress();
            var userAgent = GetClientUserAgent();

            // Create query
            var query = new GetCurrentUserQuery(userId, ipAddress, userAgent);

            // Execute query
            var user = await _authQueryService.HandleGetCurrentUserAsync(query);

            if (user == null)
            {
                _logger.LogWarning("User not found or inactive for ID: {UserId}", userId);
                return NotFound(AuthResourceAssembler.ToErrorResponse("User not found or inactive"));
            }

            _logger.LogDebug("Current user information retrieved for user: {UserId}", userId);

            // Transform to response
            var response = AuthResourceAssembler.ToResource(user);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user information");
            return StatusCode(StatusCodes.Status500InternalServerError,
                AuthResourceAssembler.ToErrorResponse("An error occurred while retrieving user information"));
        }
    }
    /// <summary>
    /// Request password reset email
    /// </summary>
    /// <param name="request">Email address for password reset</param>
    /// <returns>Success message</returns>
    /// <response code="200">Reset email sent successfully (always returned for security)</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PasswordResetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            _logger.LogInformation("Password reset request for email: {Email}", request.Email);

            // Validate model state
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid forgot password request for email: {Email}", request.Email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            // Get client information
            var ipAddress = GetClientIpAddress();
            var userAgent = GetClientUserAgent();

            // Create command
            var command = new SendPasswordResetCommand(request.Email, ipAddress, userAgent);

            // Execute command (always returns true for security)
            var result = await _authCommandService.HandleSendPasswordResetAsync(command);

            _logger.LogInformation("Password reset request processed for email: {Email}", request.Email);

            // Always return success for security (don't reveal if email exists)
            var response = new PasswordResetResponse
            {
                Message = "Si el email está registrado, recibirás un enlace para restablecer tu contraseña.",
                Success = true
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request for email: {Email}", request.Email);
            
            // Still return success for security
            var response = new PasswordResetResponse
            {
                Message = "Si el email está registrado, recibirás un enlace para restablecer tu contraseña.",
                Success = true
            };
            
            return Ok(response);
        }
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    /// <param name="request">Reset password data with token</param>
    /// <returns>Success or error message</returns>
    /// <response code="200">Password reset successfully</response>
    /// <response code="400">Invalid request data or expired token</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PasswordResetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            _logger.LogInformation("Password reset attempt for email: {Email}", request.Email);

            // Validate model state
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid reset password request for email: {Email}", request.Email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Datos de solicitud inválidos"));
            }

            // Get client information
            var ipAddress = GetClientIpAddress();
            var userAgent = GetClientUserAgent();

            // Create command
            var command = new ResetPasswordCommand(request.Token, request.Email, request.NewPassword, ipAddress, userAgent);

            // Execute command
            var result = await _authCommandService.HandleResetPasswordAsync(command);

            if (!result)
            {
                _logger.LogWarning("Password reset failed for email: {Email}", request.Email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Token inválido, expirado o datos incorrectos"));
            }

            _logger.LogInformation("Password reset successful for email: {Email}", request.Email);

            var response = new PasswordResetResponse
            {
                Message = "Contraseña restablecida exitosamente. Ahora puedes iniciar sesión con tu nueva contraseña.",
                Success = true
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for email: {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError,
                AuthResourceAssembler.ToErrorResponse("Error interno del servidor"));
        }
    }
    /// <summary>
    /// Internal endpoint: verify credentials without issuing a token (used by other microservices)
    /// </summary>
    [HttpPost("internal/verify-credentials")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyCredentials([FromBody] SignInResource resource)
    {
        var user = await _userFacade.VerifyCredentialsAsync(resource.Email, resource.Password);
        if (user == null) return Unauthorized();

        return Ok(new
        {
            user.Id,
            FirstName = user.Name.FirstName,
            LastName = user.Name.LastName,
            FullName = user.FullName,
            Email = user.CorporateEmail.Address,
            Role = user.Role.Role,
            user.IsActive,
            user.ProfileImageUrl,
            user.LastLogin
        });
    }

    #region Private Helper Methods

    private string? GetClientIpAddress()
    {
        try
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private string? GetClientUserAgent()
    {
        try
        {
            return HttpContext.Request.Headers.UserAgent.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private int GetUserIdFromClaims()
    {
        try
        {
            var userIdClaim = HttpContext.User.FindFirst("user_id")?.Value ??
                             HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
        catch
        {
            return 0;
        }
    }

    #endregion
}