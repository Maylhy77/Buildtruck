using DotNetEnv;
using BuildTruckIncidentService.Incidents.Application.ACL.Services;
using BuildTruckIncidentService.Incidents.Application.Internal;
using BuildTruckIncidentService.Incidents.Domain.Model.Commands;
using BuildTruckIncidentService.Incidents.Domain.Model.Queries;
using BuildTruckIncidentService.Incidents.Domain.Repositories;
using BuildTruckIncidentService.Incidents.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckIncidentService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckIncidentService.Shared.Infrastructure.Tokens.JWT;
using BuildTruckShared.Domain.Repositories;
using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Configuration;
using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Services;
using BuildTruckShared.Infrastructure.Interfaces.ASP.Configuration;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// ===== ROUTING & CONTROLLERS =====
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options =>
    options.Conventions.Add(new KebabCaseRouteNamingConvention()));

// ===== CORS =====
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

// ===== DATABASE =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<IncidentServiceDbContext>(options =>
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

// ===== JWT =====
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

// ===== SWAGGER =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BuildTruck Incident Service API",
        Version = "v1",
        Description = "Incidents microservice"
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

// ===== SHARED SERVICES =====
builder.Services.AddScoped<IUnitOfWork>(provider =>
    new UnitOfWork<IncidentServiceDbContext>(provider.GetRequiredService<IncidentServiceDbContext>()));

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
var cloudinarySettings = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
if (cloudinarySettings == null || !cloudinarySettings.IsValid)
    throw new InvalidOperationException("Cloudinary settings are missing or invalid.");
builder.Services.AddScoped<ICloudinaryImageService, CloudinaryImageService>();

// ===== INCIDENTS BOUNDED CONTEXT =====
builder.Services.AddScoped<IIncidentRepository>(provider =>
    new IncidentRepository(provider.GetRequiredService<IncidentServiceDbContext>()));
builder.Services.AddScoped<IIncidentFacade, IncidentFacade>();
builder.Services.AddScoped<IIncidentCommandHandler, IncidentCommandHandler>();
builder.Services.AddScoped<IIncidentQueryHandler, IncidentQueryHandler>();
builder.Services.AddScoped<ICloudinaryService, BuildTruckIncidentService.Incidents.Infrastructure.ACL.CloudinaryService>();

builder.Services.AddHttpContextAccessor();

// ===== BUILD APP =====
builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IncidentServiceDbContext>();
    try
    {
        var creator = context.GetService<IRelationalDatabaseCreator>();
        creator.CreateTables();
    }
    catch { }
    await BuildTruckIncidentService.Shared.Infrastructure.Persistence.EFC.Seeding.DatabaseSeeder.SeedAsync(context);
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
