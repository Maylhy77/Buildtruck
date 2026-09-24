using System.Text;
using BuildTruckPersonnelService.Personnel.Application.ACL.Services;
using BuildTruckPersonnelService.Personnel.Application.Internal.CommandServices;
using BuildTruckPersonnelService.Personnel.Application.Internal.QueryServices;
using BuildTruckPersonnelService.Personnel.Domain.Repositories;
using BuildTruckPersonnelService.Personnel.Domain.Services;
using BuildTruckPersonnelService.Personnel.Infrastructure.ACL;
using BuildTruckPersonnelService.Personnel.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckPersonnelService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckPersonnelService.Shared.Infrastructure.Tokens.JWT;
using BuildTruckShared.Domain.Repositories;
using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Services;
using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Configuration;
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

builder.Services.AddDbContext<PersonnelServiceDbContext>(options =>
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
        Title = "BuildTruck Personnel Service API",
        Version = "v1",
        Description = "Personnel microservice"
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

// HTTP client for ProjectService inter-service communication
builder.Services.AddHttpClient("ProjectService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ProjectService:BaseUrl"] ?? "http://buildtruck-project-service:8080");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Shared infrastructure
builder.Services.AddScoped<IUnitOfWork>(provider =>
    new UnitOfWork<PersonnelServiceDbContext>(
        provider.GetRequiredService<PersonnelServiceDbContext>()));

// Cloudinary
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<ICloudinaryImageService, CloudinaryImageService>();

// Personnel domain
builder.Services.AddScoped<IPersonnelRepository>(provider =>
    new PersonnelRepository(provider.GetRequiredService<PersonnelServiceDbContext>()));

builder.Services.AddScoped<IPersonnelCommandService, PersonnelCommandService>();
builder.Services.AddScoped<IPersonnelQueryService, PersonnelQueryService>();

// ACL services
builder.Services.AddScoped<IProjectContextService, ProjectContextService>();
builder.Services.AddScoped<ICloudinaryService>(provider =>
{
    var sharedCloudinaryService = provider.GetRequiredService<ICloudinaryImageService>();
    var logger = provider.GetRequiredService<ILogger<CloudinaryService>>();
    return new CloudinaryService(sharedCloudinaryService, logger);
});
builder.Services.AddScoped<INotificationContextService, NotificationContextService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PersonnelServiceDbContext>();
    try
    {
        var creator = context.GetService<IRelationalDatabaseCreator>();
        creator.CreateTables();
    }
    catch { }
    try
    {
        await BuildTruckPersonnelService.Shared.Infrastructure.Persistence.EFC.Seeding.DatabaseSeeder.SeedAsync(context);
    }
    catch { }
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
