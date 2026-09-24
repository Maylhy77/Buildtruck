using DotNetEnv;
using BuildTruckMachineryService.Machinery.Application.ACL.Services;
using BuildTruckMachineryService.Machinery.Application.Internal.CommandServices;
using BuildTruckMachineryService.Machinery.Application.Internal.QueryServices;
using BuildTruckMachineryService.Machinery.Domain.Repositories;
using BuildTruckMachineryService.Machinery.Domain.Services;
using BuildTruckMachineryService.Machinery.Infrastructure.ACL;
using BuildTruckMachineryService.Machinery.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckMachineryService.Projects.Application.Internal.OutboundServices;
using BuildTruckMachineryService.Projects.Infrastructure.Http;
using BuildTruckMachineryService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckMachineryService.Shared.Infrastructure.Tokens.JWT;
using BuildTruckMachineryService.Users.Application.Internal.OutboundServices;
using BuildTruckMachineryService.Users.Infrastructure.Http;
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

using MachineryCloudinaryService = BuildTruckMachineryService.Machinery.Infrastructure.ACL.CloudinaryService;
using MachineryProjectContextService = BuildTruckMachineryService.Machinery.Application.ACL.Services.IProjectContextService;
using MachineryUserContextService = BuildTruckMachineryService.Machinery.Application.ACL.Services.IUserContextService;

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

builder.Services.AddDbContext<MachineryServiceDbContext>(options =>
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
        Title = "BuildTruck Machinery Service API",
        Version = "v1",
        Description = "Machinery microservice"
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

builder.Services.AddScoped<IUnitOfWork>(provider =>
    new UnitOfWork<MachineryServiceDbContext>(provider.GetRequiredService<MachineryServiceDbContext>()));

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
var cloudinarySettings = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
if (cloudinarySettings == null || !cloudinarySettings.IsValid)
    throw new InvalidOperationException("Cloudinary settings are missing or invalid.");
builder.Services.AddScoped<ICloudinaryImageService, CloudinaryImageService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("ProjectService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ProjectService:BaseUrl"] ?? "http://buildtruck-project-service:8080");
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<IProjectFacade, HttpProjectFacade>();

builder.Services.AddHttpClient("UserService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["UserService:BaseUrl"] ?? "http://buildtruck-user-service:8080");
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<IUserFacade, HttpUserFacade>();

builder.Services.AddScoped<IMachineryRepository>(provider =>
    new MachineryRepository(provider.GetRequiredService<MachineryServiceDbContext>()));
builder.Services.AddScoped<CreateMachineryCommandHandler>();
builder.Services.AddScoped<UpdateMachineryCommandHandler>();
builder.Services.AddScoped<DeleteMachineryCommandHandler>();
builder.Services.AddScoped<IMachineryCommandService, MachineryCommandService>();
builder.Services.AddScoped<GetActiveMachineryQueryHandler>();
builder.Services.AddScoped<GetMachineryByIdQueryHandler>();
builder.Services.AddScoped<GetMachineryByProjectQueryHandler>();
builder.Services.AddScoped<IMachineryQueryService, MachineryQueryService>();
builder.Services.AddScoped<MachineryProjectContextService, ProjectContextService>();
builder.Services.AddScoped<MachineryUserContextService, UserContextService>();
builder.Services.AddScoped<ICloudinaryService>(provider =>
{
    var sharedCloudinaryService = provider.GetRequiredService<ICloudinaryImageService>();
    var logger = provider.GetRequiredService<ILogger<MachineryCloudinaryService>>();
    return new MachineryCloudinaryService(sharedCloudinaryService, logger);
});
builder.Services.AddScoped<BuildTruckMachineryService.Machinery.Application.ACL.Services.IMachineryCloudinaryService, BuildTruckMachineryService.Machinery.Infrastructure.ACL.MachineryCloudinaryService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MachineryServiceDbContext>();
    try
    {
        var creator = context.GetService<IRelationalDatabaseCreator>();
        creator.CreateTables();
    }
    catch { }
    await BuildTruckMachineryService.Shared.Infrastructure.Persistence.EFC.Seeding.DatabaseSeeder.SeedAsync(context);
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
