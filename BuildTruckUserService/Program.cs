using DotNetEnv;
using BuildTruckShared.Domain.Repositories;
using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Configuration;
using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Services;
using BuildTruckShared.Infrastructure.ExternalServices.Email.Configuration;
using BuildTruckShared.Infrastructure.ExternalServices.Email.Services;
using BuildTruckShared.Infrastructure.Interfaces.ASP.Configuration;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckUserService.Auth.Application.ACL.Services;
using BuildTruckUserService.Auth.Application.Internal.CommandServices;
using BuildTruckUserService.Auth.Application.Internal.OutboundServices;
using BuildTruckUserService.Auth.Application.Internal.QueryServices;
using BuildTruckUserService.Auth.Domain.Services;
using BuildTruckUserService.Auth.Infrastructure.ACL;
using BuildTruckUserService.Auth.Infrastructure.Tokens.JWT.Configuration;
using BuildTruckUserService.Auth.Infrastructure.Tokens.JWT.Services;
using BuildTruckUserService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckUserService.Users.Application.ACL.Services;
using BuildTruckUserService.Users.Application.Internal.CommandServices;
using BuildTruckUserService.Users.Application.Internal.OutboundServices;
using BuildTruckUserService.Users.Application.Internal.QueryServices;
using BuildTruckUserService.Users.Domain.Repositories;
using BuildTruckUserService.Users.Domain.Services;
using BuildTruckUserService.Users.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

using AuthUserContextService = BuildTruckUserService.Auth.Application.ACL.Services.IUserContextService;
using BuildTruckUserService.Shared.Infrastructure.Persistence.EFC.Seeding;

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

builder.Services.AddDbContext<UserServiceDbContext>(options =>
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
        Title = "BuildTruck User Service API",
        Version = "v1",
        Description = "Auth + Users microservice"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT token — paste without 'Bearer ' prefix",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.EnableAnnotations();
});

// ===== SHARED SERVICES ======
builder.Services.AddScoped<IUnitOfWork>(provider =>
    new UnitOfWork<UserServiceDbContext>(provider.GetRequiredService<UserServiceDbContext>()));

// Email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IGenericEmailService, GenericEmailService>();

// Cloudinary
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
var cloudinarySettings = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
if (cloudinarySettings == null || !cloudinarySettings.IsValid)
    throw new InvalidOperationException("Cloudinary settings are missing or invalid.");
builder.Services.AddScoped<ICloudinaryImageService, CloudinaryImageService>();

// ===== USERS BOUNDED CONTEXT ======
builder.Services.AddScoped<IUserRepository>(provider =>
    new UserRepository(provider.GetRequiredService<UserServiceDbContext>()));
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IImageService, ImageServiceAdapter>();
builder.Services.AddScoped<IEmailService, EmailServiceAdapter>();
builder.Services.AddScoped<IUserFacade, UserFacade>();

// ===== AUTH BOUNDED CONTEXT =====
builder.Services.AddScoped<AuthUserContextService, UserContextService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IAuthCommandService, AuthCommandService>();
builder.Services.AddScoped<IAuthQueryService, AuthQueryService>();
builder.Services.AddScoped<IAuthFacade, AuthFacade>();

builder.Services.AddHttpContextAccessor();

// ===== BUILD APP =====
builder.Services.AddHealthChecks();

var app = builder.Build();

// Ensure DB schema exists and seed initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserServiceDbContext>();
    try
    {
        var creator = context.GetService<IRelationalDatabaseCreator>();
        creator.CreateTables();
    }
    catch { }
    await DatabaseSeeder.SeedAsync(context);
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
