using BuildTruckUserService.Users.Domain.Model.Aggregates;
using BuildTruckShared.Infrastructure.ExternalServices.Email.Services;

namespace BuildTruckUserService.Users.Application.ACL.Services;

/// <summary>
/// Email service adapter for Users bounded context
/// Anti-Corruption Layer that translates Users domain concepts to generic email operations
/// </summary>
public class EmailServiceAdapter : IEmailService
{
    private readonly IGenericEmailService _genericEmailService;
    private readonly ILogger<EmailServiceAdapter> _logger;

    public EmailServiceAdapter(IGenericEmailService genericEmailService, ILogger<EmailServiceAdapter> logger)
    {
        _genericEmailService = genericEmailService;
        _logger = logger;
    }

    /// <summary>
    /// Send user credentials to their personal email address
    /// Translates User domain aggregate to generic email parameters
    /// </summary>
    public async Task SendUserCredentialsAsync(User user, string temporalPassword)
    {
        try
        {
            // ✅ Extract domain-specific logic here
            var destinationEmail = GetDestinationEmailForCredentials(user);
            var subject = "🚛 Bienvenido a BuildTruck - Credenciales de acceso";
            var htmlBody = CreateUserCredentialsEmailTemplate(user, temporalPassword);

            _logger.LogInformation("Sending user credentials via ACL to {Email} for user {FullName}", 
                destinationEmail, user.FullName);

            // ✅ Delegate to generic email service with custom content
            await _genericEmailService.SendEmailAsync(destinationEmail, subject, htmlBody);

            _logger.LogInformation("✅ User credentials sent successfully via ACL to {Email}", destinationEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send user credentials via ACL for user {UserId}", user.Id);
            // Re-throw to maintain error handling behavior in UserCommandService
            throw;
        }
    }

    /// <summary>
    /// Domain-specific logic: Determine the best email address for sending credentials
    /// Priority: Personal Email > Corporate Email
    /// </summary>
    private static string GetDestinationEmailForCredentials(User user)
    {
        // ✅ This encapsulates Users domain knowledge about email preferences
        return user.PersonalEmail ?? user.Email;
    }

    /// <summary>
    /// Create Users-specific email template for credentials
    /// Shows corporate email for login while sending to personal email
    /// </summary>
    private static string CreateUserCredentialsEmailTemplate(User user, string temporalPassword)
    {
        var destinationEmail = user.PersonalEmail ?? user.Email;
        var loginEmail = user.Email; // ✅ Always show corporate email for login
        var fullName = user.FullName;

        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <title>Bienvenido a BuildTruck</title>
        </head>
        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
            <div style='background: linear-gradient(135deg, #f97316, #ea580c); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                <h1 style='color: white; margin: 0; font-size: 28px;'>🚛 BuildTruck</h1>
                <p style='color: white; margin: 10px 0 0 0; font-size: 16px;'>Plataforma de Gestión de Construcción</p>
            </div>
            
            <div style='background: white; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
                <h2 style='color: #f97316; margin-top: 0;'>¡Bienvenido a BuildTruck, {fullName}! 👋</h2>
                
                <p>Tu cuenta ha sido creada exitosamente. A continuación encontrarás tus credenciales de acceso:</p>
                
                <div style='background: #f3f4f6; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='margin-top: 0; color: #374151;'>📧 Credenciales de Acceso:</h3>
                    <p style='margin: 5px 0;'><strong>Email corporativo (para login):</strong> {loginEmail}</p>
                    <p style='margin: 5px 0;'><strong>Contraseña temporal:</strong> <code style='background: #e5e7eb; padding: 4px 8px; border-radius: 4px; font-family: monospace;'>{temporalPassword}</code></p>
                </div>
                
                {(user.PersonalEmail != null ? $@"
                <div style='background: #e0f2fe; border-left: 4px solid #0284c7; padding: 15px; margin: 20px 0;'>
                    <h4 style='margin-top: 0; color: #0c4a6e;'>📮 Información:</h4>
                    <p style='margin: 5px 0;'>Este email se envió a tu dirección personal: <strong>{destinationEmail}</strong></p>
                    <p style='margin: 5px 0;'>Para iniciar sesión usa tu email corporativo: <strong>{loginEmail}</strong></p>
                </div>" : "")}
                
                <div style='background: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; margin: 20px 0;'>
                    <h4 style='margin-top: 0; color: #92400e;'>⚠️ Importante:</h4>
                    <ul style='margin: 10px 0; padding-left: 20px;'>
                        <li>Esta es una contraseña temporal generada automáticamente</li>
                        <li>Por seguridad, <strong>debes cambiarla</strong> en tu primer inicio de sesión</li>
                        <li>Usa una contraseña segura con al menos 8 caracteres, mayúsculas, minúsculas, números y símbolos</li>
                        <li><strong>Inicia sesión con tu email corporativo:</strong> {loginEmail}</li>
                    </ul>
                </div>
                
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='http://localhost:3000/login' style='background: #f97316; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold;'>🚀 Acceder a BuildTruck</a>
                </div>
                
                <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 30px 0;'>
                
                <p style='color: #6b7280; font-size: 14px; text-align: center;'>
                    Si tienes alguna pregunta, contacta al administrador del sistema.<br>
                    Este email fue enviado automáticamente, no responder.
                </p>
            </div>
        </body>
        </html>";
    }
    
    /// <summary>
    /// Send password reset email with token (Domain-specific logic)
    /// </summary>
    // En tu EmailServiceAdapter.cs, método SendPasswordResetEmailAsync:

    public async Task SendPasswordResetEmailAsync(User user, string resetToken)
    {
        try
        {
            _logger.LogInformation("Sending password reset email for user {UserId} via ACL", user.Id);

            var subject = "Restablecer contraseña - BuildTruck";
            var resetUrl = $"https://buildtruck-99bc0.web.app/reset-password?token={resetToken}&email={Uri.EscapeDataString(user.Email)}";
        
            // ✅ CAMBIO: Determinar email de destino
            var destinationEmail = GetBestEmailForPasswordReset(user);
            
            var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <title>Restablecer Contraseña - BuildTruck</title>
            </head>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background: linear-gradient(135deg, #f97316, #ea580c); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                    <h1 style='color: white; margin: 0; font-size: 28px;'>🚛 BuildTruck</h1>
                    <p style='color: white; margin: 10px 0 0 0; font-size: 16px;'>Restablecer Contraseña</p>
                </div>
                
                <div style='background: white; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
                    <h2 style='color: #f97316; margin-top: 0;'>Solicitud de restablecimiento de contraseña</h2>
                    
                    <p>Hola <strong>{user.FullName}</strong>,</p>
                    
                    <p>Has solicitado restablecer tu contraseña en BuildTruck.</p>
                    
                    {(destinationEmail != user.Email ? $@"
                    <div style='background: #e0f2fe; border-left: 4px solid #0284c7; padding: 15px; margin: 20px 0;'>
                        <h4 style='margin-top: 0; color: #0c4a6e;'>📮 Información importante:</h4>
                        <p style='margin: 5px 0;'>Este email se envió a tu dirección personal: <strong>{destinationEmail}</strong></p>
                        <p style='margin: 5px 0;'>Para iniciar sesión usa tu email corporativo: <strong>{user.Email}</strong></p>
                    </div>" : "")}
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetUrl}' style='background-color: #f97316; color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px; display: inline-block;'>🔐 Restablecer Contraseña</a>
                    </div>
                    
                    <div style='background: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; margin: 20px 0;'>
                        <h4 style='margin-top: 0; color: #92400e;'>⏰ Importante:</h4>
                        <ul style='margin: 10px 0; padding-left: 20px;'>
                            <li>Este enlace expirará en <strong>1 hora</strong> por seguridad</li>
                            <li>Si no solicitaste este cambio, puedes ignorar este email</li>
                            <li>Para iniciar sesión, <strong>siempre usa tu email corporativo: {user.Email}</strong></li>
                        </ul>
                    </div>
                    
                    <p>Si el botón no funciona, copia y pega este enlace en tu navegador:</p>
                    <p style='word-break: break-all; background: #f3f4f6; padding: 10px; border-radius: 4px; font-family: monospace; font-size: 12px;'>{resetUrl}</p>
                    
                    <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 30px 0;'>
                    
                    <p style='color: #6b7280; font-size: 14px; text-align: center;'>
                        Saludos,<br>
                        <strong>Equipo BuildTruck</strong><br>
                        Este email fue enviado automáticamente, no responder.
                    </p>
                </div>
            </body>
            </html>";

            // ✅ CAMBIO: Enviar al email más apropiado
            await _genericEmailService.SendEmailAsync(destinationEmail, subject, body);

            _logger.LogInformation("✅ Password reset email sent successfully for user {UserId} to {Email}", 
                user.Id, destinationEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send password reset email for user {UserId} via ACL", user.Id);
            throw new InvalidOperationException($"Failed to send password reset email to {user.FullName}: {ex.Message}");
        }
    }
    private static string GetBestEmailForPasswordReset(User user)
    {
        // ✅ Si tiene email personal, usarlo. Si no, usar el corporativo
        return !string.IsNullOrWhiteSpace(user.PersonalEmail) 
            ? user.PersonalEmail 
            : user.Email;
    }
}