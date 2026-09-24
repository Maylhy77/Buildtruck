using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using BuildTruckNotificationService.Notifications.Domain.Repositories;
using BuildTruckNotificationService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckNotificationService.Notifications.Infrastructure.Persistence.EFC.Repositories;

public class NotificationPreferenceRepository
    : BaseRepository<NotificationPreference, NotificationServiceDbContext>, INotificationPreferenceRepository
{
    public NotificationPreferenceRepository(NotificationServiceDbContext context) : base(context) { }

    public async Task<IEnumerable<NotificationPreference>> FindByUserIdAsync(int userId)
    {
        return await Context.Set<NotificationPreference>()
            .Where(np => np.UserId == userId)
            .ToListAsync();
    }

    public async Task<NotificationPreference?> FindByUserIdAndContextAsync(int userId, NotificationContext context)
    {
        return await Context.Set<NotificationPreference>()
            .FirstOrDefaultAsync(np => np.UserId == userId && np.Context == context);
    }

    public async Task<bool> ExistsByUserIdAndContextAsync(int userId, NotificationContext context)
    {
        return await Context.Set<NotificationPreference>()
            .AnyAsync(np => np.UserId == userId && np.Context == context);
    }
}
