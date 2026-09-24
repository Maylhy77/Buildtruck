using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using BuildTruckProjectService.Projects.Application.Internal.OutboundServices;
using BuildTruckProjectService.Projects.Domain.Model.Aggregates;
using BuildTruckProjectService.Projects.Domain.Model.ValueObjects;
using BuildTruckProjectService.Projects.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckProjectService.Projects.Interfaces.REST.Controllers;
using BuildTruckProjectService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckShared.Domain.Repositories;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckUserService.Auth.Application.Internal.CommandServices;
using BuildTruckUserService.Auth.Application.Internal.QueryServices;
using BuildTruckUserService.Auth.Domain.Services;
using BuildTruckUserService.Auth.Infrastructure.Tokens.JWT.Services;
using BuildTruckUserService.Auth.Interfaces.REST.Controllers;
using BuildTruckUserService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckUserService.Users.Application.ACL.Services;
using BuildTruckUserService.Users.Application.Internal.OutboundServices;
using BuildTruckUserService.Users.Domain.Model.Aggregates;
using BuildTruckUserService.Users.Domain.Repositories;
using BuildTruckUserService.Users.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using AuthUserContextService = BuildTruckUserService.Auth.Application.ACL.Services.IUserContextService;
using AuthUserContextServiceImpl = BuildTruckUserService.Auth.Infrastructure.ACL.UserContextService;
using UserTokenSettings = BuildTruckUserService.Auth.Infrastructure.Tokens.JWT.Configuration.TokenSettings;

namespace BuildTruckBackend.Tests;

public class IntegrationTests
{
    private const string JwtSecret = "buildtruck-test-secret-key-for-integration-tests-12345";
    private const string JwtIssuer = "BuildTruckBack";
    private const string JwtAudience = "BuildTruckBack-Users";

    [Fact]
    public async Task UserService_LoginEndpoint_ReturnsJwtForSeededManager()
    {
        // Prueba integral: verifica el flujo HTTP real de login del microservicio Auth + Users hasta la emision de JWT.
        using var server = CreateUserServiceServer();
        using var client = server.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "ana.torres@buildtruck.com",
            password = "Manager123!"
        });

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(json.RootElement.TryGetProperty("token", out var token));
        Assert.False(string.IsNullOrWhiteSpace(token.GetString()));
        Assert.Equal("Manager", json.RootElement.GetProperty("user").GetProperty("role").GetString());
        // Fin prueba integral.
    }

    [Fact]
    public async Task ProjectService_InternalEndpoints_ReturnProjectExistenceAndAccess()
    {
        // Prueba integral: verifica endpoints HTTP internos de Projects para existencia de obra y acceso de usuario con JWT.
        using var server = CreateProjectServiceServer();
        const int projectId = 101;
        using var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwtToken(userId: 7, role: "Manager"));

        var existsResponse = await client.GetAsync($"/api/v1/projects/exists/{projectId}");
        var accessResponse = await client.GetAsync($"/api/v1/projects/user-access?userId=7&projectId={projectId}");

        using var existsJson = JsonDocument.Parse(await existsResponse.Content.ReadAsStringAsync());
        using var accessJson = JsonDocument.Parse(await accessResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, existsResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, accessResponse.StatusCode);
        Assert.True(existsJson.RootElement.GetProperty("exists").GetBoolean());
        Assert.True(accessJson.RootElement.GetProperty("hasAccess").GetBoolean());
        // Fin prueba integral.
    }

    private static TestServer CreateUserServiceServer()
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddControllers().AddApplicationPart(typeof(AuthController).Assembly);
                services.AddLogging();

                services.AddSingleton<IUserFacade, TestUserFacade>();
                services.AddScoped<AuthUserContextService, AuthUserContextServiceImpl>();
                services.AddScoped<TokenService>();
                services.AddScoped<IAuthCommandService, AuthCommandService>();
                services.AddScoped<IAuthQueryService, AuthQueryService>();
                services.AddSingleton<IOptions<UserTokenSettings>>(_ => Options.Create(new UserTokenSettings
                {
                    SecretKey = JwtSecret,
                    Issuer = JwtIssuer,
                    Audience = JwtAudience,
                    ExpirationHours = 8,
                    ClockSkewMinutes = 5
                }));
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => endpoints.MapControllers());
            });

        return new TestServer(builder);
    }

    private static TestServer CreateProjectServiceServer()
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddControllers().AddApplicationPart(typeof(ProjectsInternalController).Assembly);
                services.AddLogging();

                services.AddSingleton<IProjectFacade, TestProjectFacade>();

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = TokenValidationParameters();
                    });
                services.AddAuthorization();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseEndpoints(endpoints => endpoints.MapControllers());
            });

        return new TestServer(builder);
    }

    private static TokenValidationParameters TokenValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = JwtIssuer,
            ValidateAudience = true,
            ValidAudience = JwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    }

    private static void SetEntityId(object entity, int id)
    {
        var backingField = entity.GetType().GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        backingField?.SetValue(entity, id);
    }

    private static string CreateJwtToken(int userId, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim("user_id", userId.ToString()),
            new Claim("role", role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class NoOpEmailService : IEmailService
    {
        public Task SendUserCredentialsAsync(User user, string temporalPassword) => Task.CompletedTask;

        public Task SendPasswordResetEmailAsync(User user, string resetToken) => Task.CompletedTask;
    }

    private sealed class NoOpImageService : IImageService
    {
        public Task<string> UploadUserProfileImageAsync(User user, byte[] imageBytes, string fileName) =>
            Task.FromResult("https://example.com/profile.png");

        public Task<bool> DeleteUserProfileImageAsync(User user) => Task.FromResult(true);

        public string GetUserProfileImageUrl(User user, int size = 200) =>
            user.ProfileImageUrl ?? $"https://example.com/avatar-{size}.png";

        public (bool IsValid, string ErrorMessage) ValidateUserProfileImage(byte[] imageBytes, string fileName) =>
            (true, string.Empty);
    }

    private sealed class TestUserFacade : IUserFacade
    {
        private readonly User _manager;

        public TestUserFacade()
        {
            _manager = new User(
                "Ana",
                "Torres",
                "Manager",
                BCrypt.Net.BCrypt.HashPassword("Manager123!"),
                personalEmail: "ana.torres.personal@gmail.com",
                phone: "987654322");
            SetEntityId(_manager, 7);
        }

        public Task<User?> VerifyCredentialsAsync(string email, string password)
        {
            var validEmail = string.Equals(email, _manager.Email, StringComparison.OrdinalIgnoreCase);
            var validPassword = BCrypt.Net.BCrypt.Verify(password, _manager.PasswordHash);
            return Task.FromResult<User?>(validEmail && validPassword ? _manager : null);
        }

        public Task<User?> FindByEmailAsync(string email) =>
            Task.FromResult<User?>(string.Equals(email, _manager.Email, StringComparison.OrdinalIgnoreCase) ? _manager : null);

        public Task<User?> FindByIdAsync(int userId) =>
            Task.FromResult<User?>(userId == _manager.Id ? _manager : null);

        public Task<bool> UpdateLastLoginAsync(int userId) => Task.FromResult(userId == _manager.Id);

        public Task<bool> IsActiveUserAsync(string email) =>
            Task.FromResult(string.Equals(email, _manager.Email, StringComparison.OrdinalIgnoreCase) && _manager.IsActive);

        public Task SendPasswordResetEmailAsync(int userId, string email, string fullName, string resetToken) =>
            Task.CompletedTask;

        public Task<string> GetUserProfileImageUrlAsync(int userId, int size = 200) =>
            Task.FromResult($"https://example.com/avatar-{size}.png");

        public Task<bool> ResetUserPasswordAsync(int userId, string newPassword) =>
            Task.FromResult(userId == _manager.Id);
    }

    private sealed class TestProjectFacade : IProjectFacade
    {
        private static readonly ProjectInfo Project = new()
        {
            Id = 101,
            Name = "Edificio Integracion",
            Description = "Obra creada para pruebas integrales",
            Location = "Lima, Peru",
            State = "Planificado",
            ManagerId = 7,
            SupervisorId = 9,
            StartDate = DateTime.Now.Date.AddDays(15),
            IsActive = false,
            HasSupervisor = true,
            IsReadyToStart = true
        };

        public Task<bool> ExistsByIdAsync(int projectId) => Task.FromResult(projectId == Project.Id);

        public Task<ProjectInfo?> GetProjectByIdAsync(int projectId) =>
            Task.FromResult<ProjectInfo?>(projectId == Project.Id ? Project : null);

        public Task<bool> UserHasAccessToProjectAsync(int userId, int projectId) =>
            Task.FromResult(projectId == Project.Id && userId == Project.ManagerId);

        public Task<List<ProjectInfo>> GetProjectsByUserAsync(int userId) =>
            Task.FromResult(userId == Project.ManagerId || userId == Project.SupervisorId ? [Project] : new List<ProjectInfo>());

        public Task<List<ProjectInfo>> GetProjectsByManagerAsync(int managerId) =>
            Task.FromResult(managerId == Project.ManagerId ? [Project] : new List<ProjectInfo>());

        public Task<List<ProjectInfo>> GetProjectsBySupervisorAsync(int supervisorId) =>
            Task.FromResult(supervisorId == Project.SupervisorId ? [Project] : new List<ProjectInfo>());

        public Task<int> GetProjectCountByStateAsync(string state) =>
            Task.FromResult(string.Equals(state, Project.State, StringComparison.OrdinalIgnoreCase) ? 1 : 0);

        public Task<int> GetActiveProjectsCountAsync() => Task.FromResult(0);

        public Task<List<ProjectInfo>> GetProjectsByStateAsync(string state) =>
            Task.FromResult(string.Equals(state, Project.State, StringComparison.OrdinalIgnoreCase) ? [Project] : new List<ProjectInfo>());

        public Task<List<ProjectInfo>> GetAllProjectsAsync() => Task.FromResult(new List<ProjectInfo> { Project });
    }
}
