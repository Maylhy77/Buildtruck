using System.Text;
using BuildTruckNotificationService.Notifications.Application.ACL;
using BuildTruckNotificationService.Notifications.Application.Internal.CommandServices;
using BuildTruckNotificationService.Notifications.Application.Internal.OutboundServices;
using BuildTruckNotificationService.Notifications.Application.Internal.QueryServices;
using BuildTruckNotificationService.Notifications.Domain.Repositories;
using BuildTruckNotificationService.Notifications.Domain.Services;
using BuildTruckNotificationService.Notifications.Infrastructure.ACL;
using BuildTruckNotificationService.Notifications.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckNotificationService.Notifications.Infrastructure.Services;
using BuildTruckNotificationService.Notifications.Interfaces.WebSocket;
using BuildTruckNotificationService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckNotificationService.Shared.Infrastructure.Tokens.JWT;
using BuildTruckShared.Domain.Repositories;
using BuildTruckShared.Infrastructure.ExternalServices.Email.Configuration;
using BuildTruckShared.Infrastructure.ExternalServices.Email.Services;
using BuildTruckShared.Infrastructure.Interfaces.ASP.Configuration;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using BuildTruckNotificationService.Notifications.Application.ACL.Services;
using BuildTruckNotificationService.Notifications.Infrastructure.Messaging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options =>
    options.Conventions.Add(new KebabCaseRouteNamingConvention()));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:8080",
                "http://localhost:3000",
                "https://buildtruck-99bc0.web.app",
                "https://buildtruck-99bc0.firebaseapp.com",
                "https://buildtruck.netlify.app",
                "https://a160c5e0-b357-45c1-952c-1a8cc886f536.cfargotunnel.com",
                "https://guitars-talk-seasonal-annoying.trycloudflare.com")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<NotificationServiceDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    else
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Error);
});

builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));
var tokenSettings = builder.Configuration.GetSection("TokenSettings").Get<TokenSettings>();
if (tokenSettings == null || !tokenSettings.IsValid())
    throw new InvalidOperationException("JWT TokenSettings are missing or invalid");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = tokenSettings.ValidateIssuerSigningKey,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.SecretKey)),
            ValidateIssuer = tokenSettings.ValidateIssuer,
            ValidIssuer = tokenSettings.Issuer,
            ValidateAudience = tokenSettings.ValidateAudience,
            ValidAudience = tokenSettings.Audience,
            ValidateLifetime = tokenSettings.ValidateLifetime,
            ClockSkew = TimeSpan.FromMinutes(tokenSettings.ClockSkewMinutes)
        };

        // Allow JWT in WebSocket query string for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BuildTruck Notification Service API",
        Version = "v1",
        Description = "Notification microservice with real-time WebSocket support"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT token - paste without 'Bearer ' prefix",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.EnableAnnotations();
});

// SignalR
builder.Services.AddSignalR();

// HTTP clients for inter-service communication
var httpTimeout = TimeSpan.FromSeconds(10);

builder.Services.AddHttpClient("UserService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["UserService:BaseUrl"] ?? "http://buildtruck-backend:8080");
    client.Timeout = httpTimeout;
});

builder.Services.AddHttpClient("ProjectService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ProjectService:BaseUrl"] ?? "http://buildtruck-project-service:8080");
    client.Timeout = httpTimeout;
});

builder.Services.AddHttpClient("MachineryService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["MachineryService:BaseUrl"] ?? "http://buildtruck-machinery-service:8080");
    client.Timeout = httpTimeout;
});

builder.Services.AddHttpClient("IncidentService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["IncidentService:BaseUrl"] ?? "http://buildtruck-incident-service:8080");
    client.Timeout = httpTimeout;
});

builder.Services.AddHttpClient("PersonnelService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["PersonnelService:BaseUrl"] ?? "http://buildtruck-personnel-service:8080");
    client.Timeout = httpTimeout;
});

builder.Services.AddHttpClient("ConfigurationService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ConfigurationService:BaseUrl"] ?? "http://buildtruck-configuration-service:8080");
    client.Timeout = httpTimeout;
});

builder.Services.AddHttpClient("BackendService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BackendService:BaseUrl"] ?? "http://buildtruck-backend:8080");
    client.Timeout = httpTimeout;
});

// Email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IGenericEmailService, GenericEmailService>();

// Shared infrastructure
builder.Services.AddScoped<IUnitOfWork>(provider =>
    new UnitOfWork<NotificationServiceDbContext>(
        provider.GetRequiredService<NotificationServiceDbContext>()));

// Domain repositories
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationDeliveryRepository, NotificationDeliveryRepository>();
builder.Services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();

// Domain services
// ===== MESSAGING (RabbitMQ) =====
// Sin broker, el publicador nulo devuelve false y la entrega vuelve a ser en linea.
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));
var rabbitEnabled = builder.Configuration.GetValue("RabbitMq:Enabled", false);

if (rabbitEnabled)
{
    builder.Services.AddSingleton<RabbitMqConnection>();
    builder.Services.AddSingleton<INotificationQueuePublisher, RabbitMqNotificationPublisher>();
    builder.Services.AddHostedService<NotificationDeliveryConsumer>();
}
else
{
    builder.Services.AddSingleton<INotificationQueuePublisher, NullNotificationQueuePublisher>();
}

builder.Services.AddScoped<INotificationCommandService, NotificationCommandService>();
builder.Services.AddScoped<INotificationQueryService, NotificationQueryService>();
builder.Services.AddScoped<INotificationDeliveryService, NotificationDeliveryCommandService>();
builder.Services.AddScoped<INotificationPreferenceCommandService, NotificationPreferenceCommandService>();
builder.Services.AddScoped<INotificationPreferenceQueryService, NotificationPreferenceQueryService>();

// Application facade
builder.Services.AddScoped<INotificationFacade, NotificationFacade>();

// ACL services
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IProjectContextService, ProjectContextService>();
builder.Services.AddScoped<IMaterialContextService, MaterialContextService>();
builder.Services.AddScoped<IMachineryContextService, MachineryContextService>();
builder.Services.AddScoped<IIncidentContextService, IncidentContextService>();
builder.Services.AddScoped<IPersonnelContextService, PersonnelContextService>();
builder.Services.AddScoped<IConfigurationContextService, ConfigurationContextService>();
builder.Services.AddScoped<ISharedEmailService, SharedEmailService>();
builder.Services.AddScoped<IWebSocketService, WebSocketService>();

// Background service
builder.Services.AddHostedService<NotificationBackgroundService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NotificationServiceDbContext>();
    try
    {
        var creator = context.GetService<IRelationalDatabaseCreator>();
        creator.CreateTables();
    }
    catch
    {
        // Tables already exist in the shared database.
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAllPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHealthChecks("/health");
app.Run();
