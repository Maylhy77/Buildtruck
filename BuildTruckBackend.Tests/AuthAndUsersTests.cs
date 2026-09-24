using BuildTruckUserService.Auth.Domain.Model.Commands;
using BuildTruckUserService.Users.Domain.Model.Aggregates;
using BuildTruckUserService.Users.Domain.Model.Commands;
using Xunit;

namespace BuildTruckBackend.Tests;

public class AuthAndUsersTests
{
    [Fact]
    public void SignInCommand_NormalizesEmail_AndKeepsAuditData()
    {
        // Prueba unitaria: verifica que el inicio de sesion normalice el email, conserve auditoria y no exponga password.
        var command = new SignInCommand(
            "  MANAGER@BUILdTruck.COM  ",
            "Temporal123!",
            "127.0.0.1",
            "BuildTruck mobile client");

        Assert.True(command.IsValid());
        Assert.True(command.HasAuditInfo());
        Assert.Equal("manager@buildtruck.com", command.Email);
        Assert.Contains("127.0.0.1", command.GetAuditString());
        Assert.DoesNotContain("Temporal123!", command.GetAuditString());
        // Fin prueba unitaria.
    }

    [Fact]
    public void UserCreatedFromCommand_GeneratesCorporateProfileData()
    {
        // Prueba unitaria: verifica la creacion de perfil de usuario y la generacion de email corporativo.
        var command = new CreateUserCommand(
            "Luis",
            "Rodriguez",
            "Manager",
            "luis.personal@example.com",
            "999888777");

        var user = new User(command, "hashed-password");

        Assert.True(user.IsActive);
        Assert.Equal("Luis Rodriguez", user.FullName);
        Assert.Equal("LR", user.Initials);
        Assert.Equal("luis.rodriguez@buildtruck.com", user.Email);
        Assert.Equal("luis.personal@example.com", user.PersonalEmail);
        Assert.Equal("Manager", user.Role.Role);
        // Fin prueba unitaria.
    }

    [Fact]
    public void UpdateInfo_UpdatesProfile_AndRegeneratesCorporateEmail()
    {
        // Prueba unitaria: verifica que editar el perfil actualice datos y regenere el email corporativo.
        var user = new User("Luis", "Rodriguez", "Worker", "hashed-password");

        user.UpdateInfo(
            firstName: "Maria",
            lastName: "Gomez",
            personalEmail: "maria.personal@example.com",
            phone: "955444333",
            role: "Supervisor");

        Assert.Equal("Maria Gomez", user.FullName);
        Assert.Equal("maria.gomez@buildtruck.com", user.Email);
        Assert.Equal("maria.personal@example.com", user.PersonalEmail);
        Assert.Equal("955444333", user.Phone);
        Assert.Equal("Supervisor", user.Role.Role);
        // Fin prueba unitaria.
    }
}
