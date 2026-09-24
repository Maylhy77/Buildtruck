using BuildTruckNotificationService.Notifications.Application.ACL;
using BuildTruckNotificationService.Notifications.Application.Internal.OutboundServices;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using BuildTruckNotificationService.Notifications.Domain.Repositories;
using BuildTruckNotificationService.Notifications.Domain.Services;

namespace BuildTruckNotificationService.Notifications.Infrastructure.Services;

public class NotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationBackgroundService> _logger;

    public NotificationBackgroundService(IServiceProvider serviceProvider, ILogger<NotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Background Service iniciado");
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                _logger.LogInformation("Iniciando verificaciones - {Time}", DateTime.Now);
                await ExecuteDailyChecks(scope.ServiceProvider);
                _logger.LogInformation("Verificaciones completadas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en verificaciones del background service");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task ExecuteDailyChecks(IServiceProvider sp)
    {
        await CheckActiveProjects(sp);
        await CheckLowStockMaterials(sp);
        await CheckMachineryStatus(sp);
        await CheckOpenIncidents(sp);
        await CheckPersonnelActivity(sp);
        await RetryFailedNotifications(sp);
        await SendDailyDigests(sp);
    }

    private async Task CheckActiveProjects(IServiceProvider sp)
    {
        try
        {
            var facade = sp.GetRequiredService<INotificationFacade>();
            var projectService = sp.GetRequiredService<IProjectContextService>();

            var activeProjects = await projectService.GetAllActiveProjectsAsync();
            var alertCount = 0;

            foreach (var projectId in activeProjects)
            {
                if (await projectService.ProjectExistsAsync(projectId) && DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                {
                    var projectName = await projectService.GetProjectNameAsync(projectId);
                    var managerId = await projectService.GetProjectManagerIdAsync(projectId);

                    if (managerId > 0)
                    {
                        await facade.CreateNotificationAsync(managerId,
                            NotificationType.ProjectStatusChanged, NotificationContext.Projects,
                            "Revisión Semanal de Proyecto",
                            $"Es momento de revisar el progreso del proyecto '{projectName}'.",
                            NotificationPriority.Normal, actionUrl: $"/projects/{projectId}",
                            relatedProjectId: projectId);
                        alertCount++;
                    }
                }
            }

            _logger.LogInformation("Proyectos verificados: {Count} alertas", alertCount);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error verificando proyectos activos"); }
    }

    private async Task CheckLowStockMaterials(IServiceProvider sp)
    {
        try
        {
            var facade = sp.GetRequiredService<INotificationFacade>();
            var materialService = sp.GetRequiredService<IMaterialContextService>();
            var projectService = sp.GetRequiredService<IProjectContextService>();

            var activeProjects = await projectService.GetAllActiveProjectsAsync();
            var lowStockCount = 0;

            foreach (var projectId in activeProjects)
            {
                var lowStockIds = await materialService.GetLowStockMaterialsAsync(projectId);

                if (lowStockIds.Any())
                {
                    var managerId = await projectService.GetProjectManagerIdAsync(projectId);
                    var projectName = await projectService.GetProjectNameAsync(projectId);

                    if (managerId > 0)
                    {
                        foreach (var materialId in lowStockIds)
                        {
                            var materialName = await materialService.GetMaterialNameAsync(materialId);
                            var current = await materialService.GetMaterialStockAsync(materialId);
                            var minimum = await materialService.GetMaterialMinimumStockAsync(materialId);
                            var isCritical = current <= (minimum * 0.5m);

                            await facade.CreateNotificationAsync(managerId,
                                isCritical ? NotificationType.CriticalStock : NotificationType.LowStock,
                                NotificationContext.Materials,
                                isCritical ? "Stock Critico" : "Stock Bajo",
                                $"Material '{materialName}' en proyecto '{projectName}': {current} unidades (minimo: {minimum}).",
                                isCritical ? NotificationPriority.Critical : NotificationPriority.Normal,
                                actionUrl: $"/materials/{materialId}",
                                relatedProjectId: projectId, relatedEntityId: materialId,
                                relatedEntityType: "Material");
                            lowStockCount++;
                        }
                    }
                }
            }

            _logger.LogInformation("Materiales verificados: {Count} alertas de stock bajo", lowStockCount);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error verificando stock de materiales"); }
    }

    private async Task CheckMachineryStatus(IServiceProvider sp)
    {
        try
        {
            var facade = sp.GetRequiredService<INotificationFacade>();
            var machineryService = sp.GetRequiredService<IMachineryContextService>();
            var projectService = sp.GetRequiredService<IProjectContextService>();

            var activeProjects = await projectService.GetAllActiveProjectsAsync();
            var alerts = 0;

            foreach (var projectId in activeProjects)
            {
                var count = await machineryService.GetActiveMachineryCountAsync(projectId);
                if (count < 2)
                {
                    var managerId = await projectService.GetProjectManagerIdAsync(projectId);
                    var projectName = await projectService.GetProjectNameAsync(projectId);

                    if (managerId > 0)
                    {
                        await facade.CreateNotificationAsync(managerId,
                            NotificationType.MachineryStatusChanged, NotificationContext.Machinery,
                            "Pocas Maquinas Activas",
                            $"El proyecto '{projectName}' solo tiene {count} maquinas activas.",
                            NotificationPriority.Normal, actionUrl: $"/machinery?project={projectId}",
                            relatedProjectId: projectId);
                        alerts++;
                    }
                }
            }

            _logger.LogInformation("Maquinaria verificada: {Count} alertas", alerts);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error verificando maquinaria"); }
    }

    private async Task CheckOpenIncidents(IServiceProvider sp)
    {
        try
        {
            var facade = sp.GetRequiredService<INotificationFacade>();
            var incidentService = sp.GetRequiredService<IIncidentContextService>();
            var projectService = sp.GetRequiredService<IProjectContextService>();

            var activeProjects = await projectService.GetAllActiveProjectsAsync();
            var alerts = 0;

            foreach (var projectId in activeProjects)
            {
                var openCount = await incidentService.GetOpenIncidentsCountAsync(projectId);
                if (openCount > 5)
                {
                    var managerId = await projectService.GetProjectManagerIdAsync(projectId);
                    var projectName = await projectService.GetProjectNameAsync(projectId);

                    if (managerId > 0)
                    {
                        await facade.CreateNotificationAsync(managerId,
                            NotificationType.IncidentReported, NotificationContext.Incidents,
                            "Muchos Incidentes Abiertos",
                            $"El proyecto '{projectName}' tiene {openCount} incidentes abiertos.",
                            NotificationPriority.High, actionUrl: $"/incidents?project={projectId}",
                            relatedProjectId: projectId);
                        alerts++;
                    }
                }
            }

            _logger.LogInformation("Incidentes verificados: {Count} alertas", alerts);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error verificando incidentes"); }
    }

    private async Task CheckPersonnelActivity(IServiceProvider sp)
    {
        try
        {
            var facade = sp.GetRequiredService<INotificationFacade>();
            var personnelService = sp.GetRequiredService<IPersonnelContextService>();
            var projectService = sp.GetRequiredService<IProjectContextService>();

            var activeProjects = await projectService.GetAllActiveProjectsAsync();
            var alerts = 0;

            foreach (var projectId in activeProjects)
            {
                var rate = await personnelService.GetAttendanceRateAsync(projectId);
                if (rate < 0.8m)
                {
                    var managerId = await projectService.GetProjectManagerIdAsync(projectId);
                    var projectName = await projectService.GetProjectNameAsync(projectId);

                    if (managerId > 0)
                    {
                        await facade.CreateNotificationAsync(managerId,
                            NotificationType.AttendanceAlert, NotificationContext.Personnel,
                            "Baja Asistencia del Personal",
                            $"El proyecto '{projectName}' tiene una tasa de asistencia del {rate:P1}.",
                            NotificationPriority.Normal, actionUrl: $"/personnel?project={projectId}",
                            relatedProjectId: projectId);
                        alerts++;
                    }
                }
            }

            _logger.LogInformation("Personal verificado: {Count} alertas de asistencia", alerts);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error verificando personal"); }
    }

    private async Task RetryFailedNotifications(IServiceProvider sp)
    {
        try
        {
            var deliveryService = sp.GetRequiredService<INotificationDeliveryService>();
            await deliveryService.RetryFailedDeliveriesAsync();
            _logger.LogInformation("Reintentos de notificaciones completados");
        }
        catch (Exception ex) { _logger.LogError(ex, "Error reintentando notificaciones fallidas"); }
    }

    private async Task SendDailyDigests(IServiceProvider sp)
    {
        try
        {
            var userService = sp.GetRequiredService<IUserContextService>();
            var emailService = sp.GetRequiredService<ISharedEmailService>();
            var notificationRepo = sp.GetRequiredService<INotificationRepository>();

            var admins = await userService.GetAdminUsersAsync();
            var managers = await userService.GetManagerUsersAsync();
            var allUsers = admins.Concat(managers).Distinct();
            var digestCount = 0;

            foreach (var userId in allUsers)
            {
                if (!await userService.IsUserActiveAsync(userId)) continue;

                var notifications = await notificationRepo.FindByUserIdWithFiltersAsync(
                    userId, 1, 50, null, null, null, null);

                var yesterdayNotifications = notifications
                    .Where(n => n.CreatedDate?.Date == DateTime.Now.AddDays(-1).Date)
                    .ToList();

                if (!yesterdayNotifications.Any()) continue;

                var email = await userService.GetUserEmailAsync(userId);
                var name = await userService.GetUserNameAsync(userId);

                if (!string.IsNullOrEmpty(email))
                {
                    await emailService.SendDigestEmailAsync(email, name, yesterdayNotifications,
                        DateTime.Now.AddDays(-1));
                    digestCount++;
                }
            }

            _logger.LogInformation("Resumenes diarios enviados: {Count} emails", digestCount);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error enviando resumenes diarios"); }
    }
}
