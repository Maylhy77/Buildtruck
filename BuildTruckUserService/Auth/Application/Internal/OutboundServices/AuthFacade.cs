using BuildTruckUserService.Auth.Application.ACL.Services;
using BuildTruckUserService.Auth.Application.Internal.OutboundServices;
using Microsoft.Extensions.Logging;

namespace BuildTruckUserService.Auth.Application.Internal.OutboundServices;

/// <summary>
/// Auth Facade Implementation - Minimal version
/// </summary>
/// <remarks>
/// Provides basic authentication status checks. 
/// Admin Context will use UserFacade and ProjectFacade for most statistics.
/// This facade is reserved for future security-specific features.
/// </remarks>
public class AuthFacade : IAuthFacade
{
    private readonly IUserContextService _userContextService;
    private readonly ILogger<AuthFacade> _logger;

    public AuthFacade(IUserContextService userContextService, ILogger<AuthFacade> logger)
    {
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> IsUserCurrentlyActiveAsync(int userId, TimeSpan? activityThreshold = null)
    {
        try
        {
            _logger.LogDebug("Checking if user {UserId} is currently active", userId);

            var threshold = activityThreshold ?? TimeSpan.FromHours(1); // Default 1 hour
            
            // Get user information via ACL
            var user = await _userContextService.GetUserByIdAsync(userId);
            
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return false;
            }

            if (!user.LastLogin.HasValue)
            {
                _logger.LogDebug("User {UserId} has never logged in", userId);
                return false;
            }

            var isActive = DateTime.UtcNow - user.LastLogin.Value <= threshold;
            
            _logger.LogDebug("User {UserId} activity status: {IsActive} (last login: {LastLogin})", 
                userId, isActive, user.LastLogin);
            
            return isActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user activity for user: {UserId}", userId);
            return false;
        }
    }

    // Placeholder methods - to be implemented when needed for security features
    public async Task<int> GetActiveUsersCountAsync(TimeSpan period)
    {
        _logger.LogInformation("GetActiveUsersCountAsync - Use UserFacade instead for user statistics");
        return 0;
    }

    public async Task<List<RecentLoginInfo>> GetRecentLoginsAsync(int take = 10)
    {
        _logger.LogInformation("GetRecentLoginsAsync - Use UserFacade.GetAllUsers() and filter by LastLogin instead");
        return new List<RecentLoginInfo>();
    }

    public async Task<AuthStatsInfo> GetAuthenticationStatsAsync(DateTime from, DateTime to)
    {
        _logger.LogInformation("GetAuthenticationStatsAsync - Use UserFacade for user-based statistics");
        return new AuthStatsInfo { PeriodStart = from, PeriodEnd = to };
    }

    public async Task<List<FailedLoginAttempt>> GetFailedLoginsAsync(DateTime from, int take = 50)
    {
        _logger.LogInformation("GetFailedLoginsAsync - Not implemented, will be added for security monitoring");
        return new List<FailedLoginAttempt>();
    }

    public async Task<LoginTrendsInfo> GetLoginTrendsAsync(int days = 30)
    {
        _logger.LogInformation("GetLoginTrendsAsync - Use UserFacade for login trend analysis");
        return new LoginTrendsInfo();
    }

    public async Task<List<InactiveUserInfo>> GetInactiveUsersAsync(TimeSpan inactivePeriod, int take = 20)
    {
        _logger.LogInformation("GetInactiveUsersAsync - Use UserFacade.GetAllUsers() and filter by LastLogin instead");
        return new List<InactiveUserInfo>();
    }
}

// Note: This facade is intentionally minimal.
// Admin Context should use UserFacade and ProjectFacade for most statistics.
// AuthFacade is reserved for future security-specific features like:
// - Failed login attempt tracking
// - Suspicious activity detection  
// - Authentication security events