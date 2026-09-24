using DotNetEnv;
using BuildTruckMaterialsService.Materials.Application.ACL.Services;
using BuildTruckMaterialsService.Materials.Application.Internal.CommandServices;
using BuildTruckMaterialsService.Materials.Application.Internal.OutboundServices;
using BuildTruckMaterialsService.Materials.Application.Internal.QueryServices;
using BuildTruckMaterialsService.Materials.Domain.Repositories;
using BuildTruckMaterialsService.Materials.Domain.Services;
using BuildTruckMaterialsService.Materials.Infrastructure.ACL;
using BuildTruckMaterialsService.Materials.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckMaterialsService.Materials.Interfaces.ACL;
using BuildTruckMaterialsService.Materials.Interfaces.ACL.Services;
using BuildTruckMaterialsService.Projects.Application.Internal.OutboundServices;
using BuildTruckMaterialsService.Projects.Infrastructure.Http;
using BuildTruckMaterialsService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckMaterialsService.Shared.Infrastructure.Tokens.JWT;
using BuildTruckShared.Domain.Repositories;
using BuildTruckShared.Infrastructure.Interfaces.ASP.Configuration;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckMaterialsService.Materials.Infrastructure.Cache;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

using MaterialsProjectContextService = BuildTruckMaterialsService.Materials.Application.ACL.Services.IProjectContextService;
using MaterialsUserContextService = BuildTruckMaterialsService.Materials.Application.ACL.Services.IUserContextService;

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

builder.Services.AddDbContext<MaterialsServiceDbContext>(options =>
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
        Title = "BuildTruck Materials Service API",
        Version = "v1",
        Description = "Materials microservice"
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
    new UnitOfWork<MaterialsServiceDbContext>(provider.GetRequiredService<MaterialsServiceDbContext>()));

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("ProjectService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ProjectService:BaseUrl"] ?? "http://buildtruck-project-service:8080");
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<IProjectFacade, HttpProjectFacade>();

builder.Services.AddScoped<IMaterialRepository>(provider =>
    new MaterialRepository(provider.GetRequiredService<MaterialsServiceDbContext>()));
builder.Services.AddScoped<IMaterialEntryRepository>(provider =>
    new MaterialEntryRepository(provider.GetRequiredService<MaterialsServiceDbContext>()));
builder.Services.AddScoped<IMaterialUsageRepository>(provider =>
    new MaterialUsageRepository(provider.GetRequiredService<MaterialsServiceDbContext>()));

// ===== CACHE =====
// Redis en produccion; si no hay connection string, cache en memoria del proceso.
// El codigo de Materials solo conoce IDistributedCache, nunca a Redis.
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "buildtruck:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}
builder.Services.AddScoped<IMaterialCacheService, RedisMaterialCacheService>();

builder.Services.AddScoped<IMaterialCommandService, MaterialCommandService>();
builder.Services.AddScoped<IMaterialEntryCommandService, MaterialEntryCommandService>();
builder.Services.AddScoped<IMaterialUsageCommandService, MaterialUsageCommandService>();
builder.Services.AddScoped<IMaterialQueryService, MaterialQueryService>();
builder.Services.AddScoped<IMaterialEntryQueryService, MaterialEntryQueryService>();
builder.Services.AddScoped<IMaterialUsageQueryService, MaterialUsageQueryService>();
builder.Services.AddScoped<IInventoryQueryService, InventoryQueryService>();
builder.Services.AddScoped<IMaterialFacade, MaterialFacade>();
builder.Services.AddScoped<IMaterialsContextFacade, MaterialsContextFacade>();
builder.Services.AddScoped<MaterialsProjectContextService, ProjectContextService>();
builder.Services.AddScoped<MaterialsUserContextService, UserContextService>();
builder.Services.AddScoped<INotificationContextService, NotificationContextService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MaterialsServiceDbContext>();
    try
    {
        var creator = context.GetService<IRelationalDatabaseCreator>();
        creator.CreateTables();
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
