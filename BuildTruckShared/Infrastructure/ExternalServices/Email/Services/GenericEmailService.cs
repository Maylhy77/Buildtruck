using System.Net;
using System.Net.Mail;
using BuildTruckShared.Infrastructure.ExternalServices.Email.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildTruckShared.Infrastructure.ExternalServices.Email.Services;

public class GenericEmailService : IGenericEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<GenericEmailService> _logger;

    public GenericEmailService(IOptions<EmailSettings> emailSettings, ILogger<GenericEmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string email, string fullName, string temporalPassword)
    {
        try
        {
            var subject = "Bienvenido a BuildTruck - Credenciales de acceso";
            var htmlBody = CreateWelcomeEmailTemplate(fullName, email, temporalPassword);
            await SendEmailInternalAsync(email, subject, htmlBody);
            _logger.LogInformation("Welcome email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", email);
        }
    }

    public async Task SendPasswordChangedNotificationAsync(string email, string fullName)
    {
        try
        {
            var subject = "BuildTruck - Contraseña cambiada exitosamente";
            var htmlBody = CreatePasswordChangedTemplate(fullName);
            await SendEmailAsync(email, subject, htmlBody);
            _logger.LogInformation("Password changed notification sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password changed notification to {Email}", email);
        }
    }

    public async Task SendEmailAsync(string email, string subject, string htmlBody)
    {
        try
        {
            await SendEmailInternalAsync(email, subject, htmlBody);
            _logger.LogInformation("Custom email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send custom email to {Email}", email);
            throw;
        }
    }

    private async Task SendEmailInternalAsync(string toEmail, string subject, string htmlBody)
    {
        using var mailMessage = new MailMessage();
        mailMessage.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
        mailMessage.To.Add(toEmail);
        mailMessage.Subject = subject;
        mailMessage.Body = htmlBody;
        mailMessage.IsBodyHtml = true;

        using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
        smtpClient.Credentials = new NetworkCredential(_emailSettings.SmtpUser, _emailSettings.SmtpPassword);
        smtpClient.EnableSsl = _emailSettings.EnableSsl;

        await smtpClient.SendMailAsync(mailMessage);
    }

    private static string CreateWelcomeEmailTemplate(string fullName, string email, string temporalPassword) => $@"
        <!DOCTYPE html><html><body style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;'>
        <h2>Bienvenido a BuildTruck, {fullName}!</h2>
        <p>Tu cuenta ha sido creada. Credenciales de acceso:</p>
        <p><strong>Email:</strong> {email}</p>
        <p><strong>Contraseña temporal:</strong> <code>{temporalPassword}</code></p>
        <p>Por seguridad, cambia tu contraseña en el primer inicio de sesión.</p>
        </body></html>";

    private static string CreatePasswordChangedTemplate(string fullName) => $@"
        <!DOCTYPE html><html><body style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;'>
        <h2>Contraseña cambiada exitosamente</h2>
        <p>Hola <strong>{fullName}</strong>, tu contraseña en BuildTruck ha sido cambiada el {DateTime.Now:dd/MM/yyyy HH:mm}.</p>
        <p>Si no fuiste tú, contacta al administrador inmediatamente.</p>
        </body></html>";
}
