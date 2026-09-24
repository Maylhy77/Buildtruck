using BuildTruckProjectService.Projects.Domain.Model.Aggregates;
using BuildTruckProjectService.Projects.Domain.Model.Commands;
using BuildTruckProjectService.Projects.Domain.Services;
using BuildTruckProjectService.Projects.Application.ACL.Services;
using BuildTruckProjectService.Projects.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckShared.Domain.Repositories;
using Microsoft.AspNetCore.Http;

namespace BuildTruckProjectService.Projects.Application.Internal.CommandServices;

public class ProjectCommandService : IProjectCommandService
{
    private readonly ProjectRepository _projectRepository;
    private readonly IUserContextService _userContextService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IUnitOfWork _unitOfWork;

    public ProjectCommandService(
        ProjectRepository projectRepository,
        IUserContextService userContextService,
        ICloudinaryService cloudinaryService,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _userContextService = userContextService;
        _cloudinaryService = cloudinaryService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Project?> Handle(CreateProjectCommand command)
    {
        // 1. Validate command
        if (!command.IsValid())
            throw new ArgumentException($"Invalid command: {string.Join(", ", command.GetValidationErrors())}");

        // Declare variables outside try block for cleanup access
        string? imageUrl = null;
        string? publicId = null;

        try
        {
            // 2. Validate Manager exists and has correct role
            var manager = await _userContextService.FindByIdAsync(command.ManagerId);
            if (manager == null)
                throw new ArgumentException($"Manager with ID {command.ManagerId} not found");

            if (!await _userContextService.IsManagerAsync(command.ManagerId))
                throw new ArgumentException($"User {command.ManagerId} is not a Manager");

            // 3. Validate and assign supervisor if provided or required
            int? finalSupervisorId = null;
            if (command.SupervisorId.HasValue)
            {
                // Manual supervisor assignment
                if (!await _userContextService.IsSupervisorAsync(command.SupervisorId.Value))
                    throw new ArgumentException($"User {command.SupervisorId.Value} is not a Supervisor");

                if (!await _userContextService.IsSupervisorAvailableAsync(command.SupervisorId.Value))
                    throw new ArgumentException($"Supervisor {command.SupervisorId.Value} is not available");

                finalSupervisorId = command.SupervisorId;
            }
            else if (command.RequiresSupervisorValidation())
            {
                // Auto-assign available supervisor for active projects
                finalSupervisorId = await _userContextService.FindAvailableSupervisorAsync();
                if (finalSupervisorId == null)
                    throw new InvalidOperationException("No available supervisors for active project");
            }

            // 4. Upload image if provided
            if (command.HasImageToUpload())
            {
                var fileName = $"{Guid.NewGuid()}_{command.ImageFile!.FileName}";
                var uploadResult = await _cloudinaryService.UploadImageAsync(command.ImageFile!, fileName);
                    
                if (!uploadResult.IsSuccess)
                    throw new InvalidOperationException($"Image upload failed: {uploadResult.Error}");
                    
                imageUrl = uploadResult.ImageUrl;
                publicId = uploadResult.PublicId;
            }

            // 5. Create project
            var coordinates = command.GetValidCoordinates();
            var project = new Project(
                command.Name,
                command.Description,
                command.ManagerId,
                command.Location,
                command.StartDate,
                coordinates,
                imageUrl,
                command.State);

            // 6. Assign supervisor if needed
            if (finalSupervisorId.HasValue)
            {
                project.AssignSupervisor(finalSupervisorId.Value);
            }

            // 7. Validate final project state
            if (!project.IsValid())
                throw new InvalidOperationException($"Project validation failed: {string.Join(", ", project.GetValidationErrors())}");

            // 8. Save to database
            await _projectRepository.AddAsync(project);
            await _unitOfWork.CompleteAsync();

            // 9. Update supervisor assignment via ACL (after project has ID)
            if (finalSupervisorId.HasValue)
            {
                await _userContextService.AssignSupervisorToProjectAsync(
                    finalSupervisorId.Value, 
                    project.Id);
            }

            return project;
        }
        catch (Exception ex)
        {
            // Cleanup uploaded image if project creation failed
            if (!string.IsNullOrEmpty(imageUrl))
            {
                try
                {
                    await _cloudinaryService.DeleteImageAsync(imageUrl);
                }
                catch
                {
                    // Log but don't throw - original exception is more important
                }
            }

            throw new InvalidOperationException($"Project creation failed: {ex.Message}", ex);
        }
    }

    public async Task<Project?> Handle(UpdateProjectCommand command)
    {
        // 1. Validate command
        if (!command.IsValid())
            throw new ArgumentException($"Invalid command: {string.Join(", ", command.GetValidationErrors())}");

        if (!command.HasChanges())
            throw new ArgumentException("No changes specified in update command");

        try
        {
            // 2. Find existing project
            var project = await _projectRepository.FindByIdAsync(command.ProjectId);
            if (project == null)
                throw new ArgumentException($"Project with ID {command.ProjectId} not found");

            // Store original values for potential rollback
            var originalImageUrl = project.ImageUrl;
            var originalSupervisorId = project.SupervisorId;

            // 3. Handle state change validation
            if (command.RequiresStateValidation())
            {
                var newState = command.GetValidState()!;
                
                if (!project.State.CanTransitionTo(newState))
                    throw new InvalidOperationException($"Cannot transition from {project.State} to {newState}");
                
                // Check supervisor requirement for active state
                if (newState.RequiresSupervisor() && !project.HasSupervisor && !command.SupervisorId.HasValue)
                    throw new InvalidOperationException("Cannot set project to active without supervisor");
            }

            // 4. Handle supervisor changes
            if (command.RequiresSupervisorValidation())
            {
                if (command.RemoveSupervisor)
                {
                    if (project.State.RequiresSupervisor())
                        throw new InvalidOperationException("Cannot remove supervisor from active project");
                    
                    // Release current supervisor
                    if (project.SupervisorId.HasValue)
                    {
                        await _userContextService.ReleaseSupervisorFromProjectAsync(project.SupervisorId.Value);
                        project.RemoveSupervisor();
                    }
                }
                else if (command.SupervisorId.HasValue)
                {
                    // Validate new supervisor
                    if (!await _userContextService.IsSupervisorAsync(command.SupervisorId.Value))
                        throw new ArgumentException($"User {command.SupervisorId.Value} is not a Supervisor");

                    if (!await _userContextService.IsSupervisorAvailableAsync(command.SupervisorId.Value))
                        throw new ArgumentException($"Supervisor {command.SupervisorId.Value} is not available");

                    // Release old supervisor if exists
                    if (project.SupervisorId.HasValue && project.SupervisorId != command.SupervisorId)
                    {
                        await _userContextService.ReleaseSupervisorFromProjectAsync(project.SupervisorId.Value);
                    }

                    // Assign new supervisor
                    project.ChangeSupervisor(command.SupervisorId.Value);
                    await _userContextService.AssignSupervisorToProjectAsync(command.SupervisorId.Value, project.Id);
                }
            }

            // 5. Handle image changes
            if (command.RequiresImageHandling())
            {
                if (command.RemoveImage)
                {
                    // Delete existing image
                    if (!string.IsNullOrEmpty(project.ImageUrl))
                    {
                        await _cloudinaryService.DeleteImageAsync(project.ImageUrl);
                        project.UpdateImage(null);
                    }
                }
                else if (command.HasImageToUpload())
                {
                    // Upload new image
                    var fileName = $"{project.Id}_{Guid.NewGuid()}_{command.ImageFile!.FileName}";
                    var uploadResult = await _cloudinaryService.UploadImageAsync(command.ImageFile!, fileName);
                        
                    if (!uploadResult.IsSuccess)
                        throw new InvalidOperationException($"Image upload failed: {uploadResult.Error}");

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(project.ImageUrl))
                    {
                        await _cloudinaryService.DeleteImageAsync(project.ImageUrl);
                    }

                    project.UpdateImage(uploadResult.ImageUrl);
                }
            }

            // 6. Update project details
            project.UpdateDetails(
                command.Name,
                command.Description,
                command.Location,
                command.StartDate);

            // 7. Update coordinates
            if (command.Coordinates != null)
            {
                var coordinates = command.GetValidCoordinates();
                project.UpdateCoordinates(coordinates);
            }

            // 8. Change state if specified
            if (command.RequiresStateValidation())
            {
                project.ChangeState(command.State!);
            }

            // 9. Final validation
            if (!project.IsValid())
                throw new InvalidOperationException($"Project validation failed: {string.Join(", ", project.GetValidationErrors())}");

            // 10. Save changes
            _projectRepository.Update(project);
            await _unitOfWork.CompleteAsync();
            return project;
            
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Project update failed: {ex.Message}", ex);
        }
    }

    public async Task<bool> Handle(DeleteProjectCommand command)
    {
        // 1. Validate command
        if (!command.IsValid())
            throw new ArgumentException($"Invalid command: {string.Join(", ", command.GetValidationErrors())}");

        try
        {
            // 2. Find project
            var project = await _projectRepository.FindByIdAsync(command.ProjectId);
            if (project == null)
                return false; // Project not found, consider as successful deletion

            // 3. Check if project can be deleted
            if (!command.ForceDelete && !project.CanBeDeleted())
                throw new InvalidOperationException("Project cannot be deleted - it may be completed or have dependencies");

            // 4. Validate permissions (basic check - extend as needed)
            var requester = await _userContextService.FindByIdAsync(command.RequestedByUserId);
            if (requester == null)
                throw new ArgumentException($"User {command.RequestedByUserId} not found");

            // Only allow managers of the project or admins to delete
            if (project.ManagerId != command.RequestedByUserId && 
                !await _userContextService.IsAdminAsync(command.RequestedByUserId))
                throw new UnauthorizedAccessException("Only project managers or admins can delete projects");

            // 5. Release supervisor if assigned
            if (project.SupervisorId.HasValue)
            {
                await _userContextService.ReleaseSupervisorFromProjectAsync(project.SupervisorId.Value);
            }

            // 6. Delete project image if exists
            if (!string.IsNullOrEmpty(project.ImageUrl))
            {
                try
                {
                    await _cloudinaryService.DeleteImageAsync(project.ImageUrl);
                }
                catch
                {
                    // Log warning - don't fail deletion for image cleanup issues
                }
            }

            // 7. Delete project from database
            _projectRepository.Remove(project);
            await _unitOfWork.CompleteAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Project deletion failed: {ex.Message}", ex);
        }
    }
}