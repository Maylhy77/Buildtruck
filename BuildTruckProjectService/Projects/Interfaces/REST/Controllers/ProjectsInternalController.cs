using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BuildTruckProjectService.Projects.Application.Internal.OutboundServices;

namespace BuildTruckProjectService.Projects.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/projects")]
[AllowAnonymous]
public class ProjectsInternalController(IProjectFacade projectFacade) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await projectFacade.GetProjectByIdAsync(id);
        if (project == null) return NotFound();
        return Ok(project);
    }

    [HttpGet("exists/{id:int}")]
    public async Task<IActionResult> Exists(int id)
    {
        var exists = await projectFacade.ExistsByIdAsync(id);
        return Ok(new { exists });
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var projects = await projectFacade.GetAllProjectsAsync();
        return Ok(projects);
    }

    [HttpGet("active-count")]
    public async Task<IActionResult> GetActiveCount()
    {
        var count = await projectFacade.GetActiveProjectsCountAsync();
        return Ok(new { count });
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCountByState([FromQuery] string state)
    {
        var count = await projectFacade.GetProjectCountByStateAsync(state);
        return Ok(new { count });
    }

    [HttpGet("by-state")]
    public async Task<IActionResult> GetByState([FromQuery] string state)
    {
        var projects = await projectFacade.GetProjectsByStateAsync(state);
        return Ok(projects);
    }

    [HttpGet("internal/by-manager/{managerId:int}")]
    public async Task<IActionResult> GetByManager(int managerId)
    {
        var projects = await projectFacade.GetProjectsByManagerAsync(managerId);
        return Ok(projects);
    }

    [HttpGet("internal/by-supervisor/{supervisorId:int}")]
    public async Task<IActionResult> GetBySupervisor(int supervisorId)
    {
        var projects = await projectFacade.GetProjectsBySupervisorAsync(supervisorId);
        return Ok(projects);
    }

    [HttpGet("by-user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var projects = await projectFacade.GetProjectsByUserAsync(userId);
        return Ok(projects);
    }

    [HttpGet("user-access")]
    public async Task<IActionResult> UserHasAccess([FromQuery] int userId, [FromQuery] int projectId)
    {
        var hasAccess = await projectFacade.UserHasAccessToProjectAsync(userId, projectId);
        return Ok(new { hasAccess });
    }
}
