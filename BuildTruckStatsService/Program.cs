using System.Text;
using BuildTruckStatsService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckStatsService.Shared.Infrastructure.Tokens.JWT;
using BuildTruckStatsService.Stats.Application.ACL.Services;
using BuildTruckStatsService.Stats.Application.Internal.CommandServices;
using BuildTruckStatsService.Stats.Application.Internal.QueryServices;
using BuildTruckStatsService.Stats.Domain.Repositories;
using BuildTruckStatsService.Stats.Domain.Services;
using BuildTruckStatsService.Stats.Infrastructure.ACL;
using BuildTruckStatsService.Stats.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckShared.Domain.Repositories;
using BuildTruckShared.Infrastructure.Interfaces.ASP.Configuration;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
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

builder.Services.AddDbContext<StatsServiceDbContext>(options =>
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
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BuildTruck Stats Service API",
        Version = "v1",
        Description = "Stats microservice — dashboard analytics for managers"
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

// Named HTTP clients for inter-service communication
builder.Services.AddHttpClient("UserService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["UserService:BaseUrl"] ?? "http://buildtruck-user-service:8080");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient("ProjectService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ProjectService:BaseUrl"] ?? "http://buildtruck-project-service:8080");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient("PersonnelService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["PersonnelService:BaseUrl"] ?? "http://buildtruck-personnel-service:8080");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient("MaterialsService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["MaterialsService:BaseUrl"] ?? "http://buildtruck-materials-service:8080");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient("MachineryService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["MachineryService:BaseUrl"] ?? "http://buildtruck-machinery-service:8080");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient("IncidentService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["IncidentService:BaseUrl"] ?? "http://buildtruck-incident-service:8080");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Shared infrastructure
builder.Services.AddScoped<IUnitOfWork>(provider =>
    new UnitOfWork<StatsServiceDbContext>(
        provider.GetRequiredService<StatsServiceDbContext>()));

// Stats repositories
builder.Services.AddScoped<IStatsRepository>(provider =>
    new StatsRepository(
        provider.GetRequiredService<StatsServiceDbContext>(),
        provider.GetRequiredService<ILogger<StatsRepository>>()));

builder.Services.AddScoped<IStatsHistoryRepository>(provider =>
    new StatsHistoryRepository(
        provider.GetRequiredService<StatsServiceDbContext>(),
        provider.GetRequiredService<ILogger<StatsHistoryRepository>>()));

// Stats services
builder.Services.AddScoped<IStatsCommandService, StatsCommandService>();
builder.Services.AddScoped<IStatsQueryService, StatsQueryService>();

// ACL services (all HTTP)
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IProjectContextService, ProjectContextService>();
builder.Services.AddScoped<IPersonnelContextService, PersonnelContextService>();
builder.Services.AddScoped<IMaterialContextService, MaterialContextService>();
builder.Services.AddScoped<IMachineryContextService, MachineryContextService>();
builder.Services.AddScoped<IIncidentContextService, IncidentContextService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<StatsServiceDbContext>();
    try
    {
        var creator = context.GetService<IRelationalDatabaseCreator>();
        creator.CreateTables();
    }
    catch
    {
        // Tables already exist in the shared database
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
app.MapHealthChecks("/health");
app.Run();
