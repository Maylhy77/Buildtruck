using BuildTruckProjectService.Projects.Domain.Model.Aggregates;
using BuildTruckProjectService.Projects.Domain.Model.Commands;
using BuildTruckProjectService.Projects.Domain.Model.ValueObjects;
using Xunit;

namespace BuildTruckBackend.Tests;

public class ProjectsTests
{
    [Fact]
    public void CreateProjectCommand_WithValidConstructionWorkData_IsValid()
    {
        // Prueba unitaria: verifica que el registro de una obra valida pase las reglas de negocio del backend.
        var command = new CreateProjectCommand(
            name: "Edificio Central",
            description: "Construccion de edificio multifamiliar",
            managerId: 7,
            location: "Lima, Peru",
            startDate: DateTime.Now.Date.AddDays(10),
            coordinates: new { lat = -12.0464, lng = -77.0428 },
            supervisorId: 9,
            state: "Planificado");

        Assert.True(command.IsValid());
        Assert.Empty(command.GetValidationErrors());
        Assert.True(command.IsProjectInPeru());
        Assert.NotNull(command.GetValidCoordinates());
        // Fin prueba unitaria.
    }

    [Fact]
    public void CreateProjectCommand_WithInvalidManagerAndPastDate_ReturnsValidationErrors()
    {
        // Prueba unitaria: verifica que una obra con manager invalido y fecha pasada retorne errores.
        var command = new CreateProjectCommand(
            name: "Obra Norte",
            description: "Proyecto de validacion",
            managerId: 0,
            location: "Lima, Peru",
            startDate: DateTime.Now.Date.AddDays(-5));

        var errors = command.GetValidationErrors();

        Assert.False(command.IsValid());
        Assert.Contains(errors, error => error.Contains("ManagerId"));
        Assert.Contains(errors, error => error.Contains("past"));
        // Fin prueba unitaria.
    }

    [Fact]
    public void Project_CanBeActivatedOnlyAfterSupervisorAssignment()
    {
        // Prueba unitaria: verifica que una obra solo pueda activarse cuando tiene supervisor asignado.
        var project = new Project(
            name: "Residencial Sur",
            description: "Construccion de viviendas",
            managerId: 3,
            location: "Arequipa, Peru",
            startDate: DateTime.Now.Date.AddDays(5),
            coordinates: new ProjectCoordinates(-16.4090, -71.5375),
            state: "Planificado");

        Assert.False(project.IsReadyToStart());
        Assert.Throws<InvalidOperationException>(() => project.ChangeState("Activo"));

        project.AssignSupervisor(10);
        project.ChangeState("Activo");

        Assert.True(project.IsReadyToStart());
        Assert.True(project.State.IsActive);
        Assert.Equal(10, project.SupervisorId);
        // Fin prueba unitaria.
    }

    [Fact]
    public void UpdateProjectCommand_DetectsChanges_AndRejectsInvalidProjectId()
    {
        // Prueba unitaria: verifica que editar una obra detecte cambios y rechace ids invalidos.
        var emptyUpdate = new UpdateProjectCommand(projectId: 5);
        var invalidUpdate = new UpdateProjectCommand(projectId: 0, name: "Nueva Obra");
        var validUpdate = new UpdateProjectCommand(projectId: 5, name: "Nueva Obra");

        Assert.False(emptyUpdate.HasChanges());
        Assert.False(invalidUpdate.IsValid());
        Assert.Contains(invalidUpdate.GetValidationErrors(), error => error.Contains("ProjectId"));
        Assert.True(validUpdate.HasChanges());
        Assert.True(validUpdate.IsValid());
        // Fin prueba unitaria.
    }
}
