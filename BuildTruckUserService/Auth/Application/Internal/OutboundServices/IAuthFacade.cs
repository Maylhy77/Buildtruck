using BuildTruckUserService.Auth.Domain.Model.ValueObjects;

namespace BuildTruckUserService.Auth.Application.Internal.OutboundServices;

/// <summary>
/// Auth Facade - Interface for communication between bounded contexts
/// Provides authentication statistics and monitoring for other contexts
/// </summary>
public interface IAuthFacade
{
    /// <summary>
    /// Get count of users who have logged in within the specified time period
    /// </summary>
    /// <param name="period">Time period to check (e.g., last 24 hours, 7 days)</param>
    /// <returns>Number of active users in the given period</returns>
    Task<int> GetActiveUsersCountAsync(TimeSpan period);

    /// <summary>
    /// Get recent login information for dashboard display
    /// </summary>
    /// <param name="take">Number of recent logins to retrieve (default: 10)</param>
    /// <returns>List of recent login information</returns>
    Task<List<RecentLoginInfo>> GetRecentLoginsAsync(int take = 10);

    /// <summary>
    /// Get authentication statistics for a date range
    /// </summary>
    /// <param name="from">Start date for statistics</param>
    /// <param name="to">End date for statistics</param>
    /// <returns>Authentication statistics summary</returns>
    Task<AuthStatsInfo> GetAuthenticationStatsAsync(DateTime from, DateTime to);

    /// <summary>
    /// Get failed login attempts for security monitoring
    /// </summary>
    /// <param name="from">Start date to check failed attempts</param>
    /// <param name="take">Number of failed attempts to retrieve (default: 50)</param>
    /// <returns>List of failed login attempts</returns>
    Task<List<FailedLoginAttempt>> GetFailedLoginsAsync(DateTime from, int take = 50);

    /// <summary>
    /// Check if a user is currently considered "logged in" (has recent activity)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="activityThreshold">Time threshold for considering user active (default: 1 hour)</param>
    /// <returns>True if user is currently active, false otherwise</returns>
    Task<bool> IsUserCurrentlyActiveAsync(int userId, TimeSpan? activityThreshold = null);

    /// <summary>
    /// Get login trends for admin dashboard analytics
    /// </summary>
    /// <param name="days">Number of days to analyze (default: 30)</param>
    /// <returns>Daily login counts and trends</returns>
    Task<LoginTrendsInfo> GetLoginTrendsAsync(int days = 30);

    /// <summary>
    /// Get users who haven't logged in for a specified period (inactive users)
    /// </summary>
    /// <param name="inactivePeriod">Period to consider user as inactive</param>
    /// <param name="take">Number of inactive users to retrieve</param>
    /// <returns>List of inactive users</returns>
    Task<List<InactiveUserInfo>> GetInactiveUsersAsync(TimeSpan inactivePeriod, int take = 20);
}

/// <summary>
/// Recent login information for dashboard display
/// </summary>
public record RecentLoginInfo
{
    public int UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime? LastLogin { get; init; }
    public string? IpAddress { get; init; }
}

/// <summary>
/// Authentication statistics summary
/// </summary>
public record AuthStatsInfo
{
    public int TotalLogins { get; init; }
    public int UniqueUsers { get; init; }
    public int FailedAttempts { get; init; }
    public double AverageLoginsPerDay { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
}

/// <summary>
/// Failed login attempt information for security monitoring
/// </summary>
public record FailedLoginAttempt
{
    public string Email { get; init; } = string.Empty;
    public DateTime AttemptTime { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Login trends information for analytics
/// </summary>
public record LoginTrendsInfo
{
    public List<DailyLoginCount> DailyCounts { get; init; } = new();
    public double AverageLoginsPerDay { get; init; }
    public int TotalLogins { get; init; }
    public int TrendDirection { get; init; } // -1: decreasing, 0: stable, 1: increasing
}

/// <summary>
/// Daily login count for trend analysis
/// </summary>
public record DailyLoginCount
{
    public DateTime Date { get; init; }
    public int LoginCount { get; init; }
    public int UniqueUsers { get; init; }
}

/// <summary>
/// Inactive user information
/// </summary>
public record InactiveUserInfo
{
    public int UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime? LastLogin { get; init; }
    public int DaysSinceLastLogin { get; init; }
}