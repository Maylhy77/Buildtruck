using BuildTruckNotificationService.Notifications.Application.ACL;
using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using BuildTruckShared.Infrastructure.ExternalServices.Email.Services;

namespace BuildTruckNotificationService.Notifications.Infrastructure.ACL;

public class SharedEmailService : ISharedEmailService
{
    private readonly IGenericEmailService _emailService;

    public SharedEmailService(IGenericEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendNotificationEmailAsync(string email, string fullName, NotificationType type,
        string title, string message, string? actionUrl = null)
    {
        var subject = $"BuildTruck - {title}";
        var actionButton = !string.IsNullOrWhiteSpace(actionUrl)
            ? $@"<div style='text-align:center;margin:20px 0;'>
                    <a href='{actionUrl}' style='background:#f97316;color:white;padding:12px 24px;text-decoration:none;border-radius:6px;font-weight:bold;'>Ver detalles</a>
                 </div>"
            : string.Empty;

        var html = $@"<!DOCTYPE html><html><body style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;'>
            <div style='background:linear-gradient(135deg,#f97316,#ea580c);padding:30px;text-align:center;border-radius:10px 10px 0 0;'>
                <h1 style='color:white;margin:0;'>BuildTruck</h1>
            </div>
            <div style='background:white;padding:30px;border:1px solid #e5e7eb;border-radius:0 0 10px 10px;'>
                <h2 style='color:#f97316;'>{title}</h2>
                <p>Hola <strong>{fullName}</strong>,</p>
                <div style='background:#f3f4f6;padding:20px;border-radius:8px;margin:20px 0;'>
                    <p style='margin:0;'>{message}</p>
                </div>
                {actionButton}
                <p style='color:#6b7280;font-size:14px;text-align:center;'>BuildTruck - Sistema de Gestión de Construcción</p>
            </div></body></html>";

        await _emailService.SendEmailAsync(email, subject, html);
    }

    public async Task SendDigestEmailAsync(string email, string fullName, List<Notification> notifications, DateTime date)
    {
        var subject = $"BuildTruck - Resumen diario {date:dd/MM/yyyy}";
        var items = string.Join("", notifications.Take(10).Select(n =>
            $@"<li style='margin-bottom:15px;padding:10px;background:#f9fafb;border-radius:6px;'>
                  <strong>{n.Content.Title}</strong><br>
                  <span style='color:#6b7280;font-size:14px;'>{n.Content.Message}</span><br>
                  <span style='color:#9ca3af;font-size:12px;'>{n.Context.Value}</span>
               </li>"));

        var more = notifications.Count > 10 ? $"<p>Y {notifications.Count - 10} notificaciones más...</p>" : "";

        var html = $@"<!DOCTYPE html><html><body style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;'>
            <div style='background:linear-gradient(135deg,#3b82f6,#1d4ed8);padding:30px;text-align:center;border-radius:10px 10px 0 0;'>
                <h1 style='color:white;margin:0;'>BuildTruck</h1>
                <p style='color:white;'>Resumen Diario</p>
            </div>
            <div style='background:white;padding:30px;border:1px solid #e5e7eb;border-radius:0 0 10px 10px;'>
                <h2 style='color:#3b82f6;'>Resumen del {date:dd/MM/yyyy}</h2>
                <p>Hola <strong>{fullName}</strong>, aquí tienes el resumen del día:</p>
                <ul style='list-style:none;padding:0;'>{items}</ul>
                {more}
                <p style='color:#6b7280;font-size:14px;text-align:center;'>BuildTruck - Sistema de Gestión de Construcción</p>
            </div></body></html>";

        await _emailService.SendEmailAsync(email, subject, html);
    }

    public async Task SendCriticalNotificationEmailAsync(string email, string fullName, string title,
        string message, string projectName, string? actionUrl = null)
    {
        var subject = $"BuildTruck - CRITICO: {title}";
        var actionButton = !string.IsNullOrWhiteSpace(actionUrl)
            ? $@"<div style='text-align:center;margin:20px 0;'>
                    <a href='{actionUrl}' style='background:#dc2626;color:white;padding:12px 24px;text-decoration:none;border-radius:6px;font-weight:bold;'>ACCION REQUERIDA</a>
                 </div>"
            : string.Empty;

        var html = $@"<!DOCTYPE html><html><body style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;'>
            <div style='background:linear-gradient(135deg,#dc2626,#b91c1c);padding:30px;text-align:center;border-radius:10px 10px 0 0;'>
                <h1 style='color:white;margin:0;'>NOTIFICACION CRITICA</h1>
            </div>
            <div style='background:white;padding:30px;border:1px solid #e5e7eb;border-radius:0 0 10px 10px;'>
                <h2 style='color:#dc2626;'>{title}</h2>
                <p>Hola <strong>{fullName}</strong>,</p>
                <div style='background:#fef2f2;border-left:4px solid #dc2626;padding:15px;margin:20px 0;'>
                    <p style='font-weight:bold;'>{message}</p>
                    <p><strong>Proyecto:</strong> {projectName}</p>
                    <p><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                </div>
                {actionButton}
                <p style='color:#6b7280;font-size:14px;text-align:center;'>BuildTruck - Sistema de Gestión de Construcción</p>
            </div></body></html>";

        await _emailService.SendEmailAsync(email, subject, html);
    }
}
