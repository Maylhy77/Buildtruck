using BuildTruckProjectService.Projects.Domain.Model.Aggregates;
using BuildTruckProjectService.Projects.Domain.Model.Commands;
using BuildTruckProjectService.Projects.Interfaces.REST.Resources;
using BuildTruckProjectService.Projects.Application.ACL.Services;

namespace BuildTruckProjectService.Projects.Interfaces.REST.Transform;

/// <summary>
/// Assembler for transforming between Project entities and REST resources
/// </summary>
/// <remarks>
/// Handles mapping logic and enrichment with additional data from ACL services
/// </remarks>
public class ProjectResourceAssembler
{
    private readonly IUserContextService _userContextService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<ProjectResourceAssembler> _logger;

    public ProjectResourceAssembler(
        IUserContextService userContextService,
        ICloudinaryService cloudinaryService,
        ILogger<ProjectResourceAssembler> logger)
    {
        _userContextService = userContextService;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    /// <summary>
    /// Transform CreateProjectResource to CreateProjectCommand
    /// </summary>
    public static CreateProjectCommand ToCommandFromResource(CreateProjectResource resource)
    {
        return new CreateProjectCommand(
            name: resource.Name,
            description: resource.Description,
            managerId: resource.ManagerId,
            location: resource.Location,
            startDate: resource.StartDate,
            coordinates: resource.Coordinates,
            imageFile: resource.ImageFile,
            supervisorId: resource.SupervisorId,
            state: resource.State
        );
    }

    /// <summary>
    /// Transform UpdateProjectResource to UpdateProjectCommand
    /// </summary>
    public static UpdateProjectCommand ToCommandFromResource(int projectId, UpdateProjectResource resource)
    {
        return new UpdateProjectCommand(
            projectId: projectId,
            name: resource.Name,
            description: resource.Description,
            location: resource.Location,
            startDate: resource.StartDate,
            coordinates: resource.Coordinates,
            imageFile: resource.ImageFile,
            removeImage: resource.RemoveImage,
            supervisorId: resource.SupervisorId,
            removeSupervisor: resource.RemoveSupervisor,
            state: resource.State
        );
    }

    /// <summary>
    /// Transform Project entity to ProjectResource with enriched data
    /// </summary>
    public async Task<ProjectResource> ToResourceFromEntityAsync(Project project)
    {
        try
        {
            // Get manager information
            var manager = await _userContextService.FindByIdAsync(project.ManagerId);
            
            // Get supervisor information if assigned
            UserDto? supervisor = null;
            if (project.SupervisorId.HasValue)
            {
                supervisor = await _userContextService.FindByIdAsync(project.SupervisorId.Value);
            }

            // Generate thumbnail URL if project has image
            string? thumbnailUrl = null;
            if (!string.IsNullOrEmpty(project.ImageUrl))
            {
                try
                {
                    // Extract public ID from URL for thumbnail generation
                    var publicId = ExtractPublicIdFromUrl(project.ImageUrl);
                    if (!string.IsNullOrEmpty(publicId))
                    {
                        thumbnailUrl = _cloudinaryService.GenerateThumbnailUrl(publicId, 800); 
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate thumbnail for project {ProjectId}", project.Id);
                }
            }

            // Transform coordinates
            ProjectCoordinatesResource? coordinatesResource = null;
            if (project.Coordinates != null && !project.Coordinates.IsZero())
            {
                coordinatesResource = new ProjectCoordinatesResource
                {
                    Lat = project.Coordinates.Latitude,
                    Lng = project.Coordinates.Longitude,
                    IsInPeru = project.Coordinates.IsInPeru(),
                    DistanceToLima = project.Coordinates.DistanceToLima(),
                    GoogleMapsUrl = project.Coordinates.ToGoogleMapsUrl(),
                    DecimalString = project.Coordinates.ToDecimalString()
                };
            }

            return new ProjectResource
            {
                Id = project.Id,
                Name = project.ProjectName,
                Description = project.ProjectDescription,
                ImageUrl = project.ImageUrl,
                ThumbnailUrl = thumbnailUrl,
                ManagerId = project.ManagerId,
                ManagerName = manager?.FullName ?? "Unknown Manager",
                ManagerEmail = manager?.Email ?? "",
                Location = project.ProjectLocation,
                ShortLocation = project.Location.GetShortLocation(50),
                Coordinates = coordinatesResource,
                StartDate = project.StartDate,
                SupervisorId = project.SupervisorId,
                SupervisorName = supervisor?.FullName,
                SupervisorEmail = supervisor?.Email,
                State = project.ProjectState,
                StateDisplayName = project.State.GetDisplayName(),
                StateColor = project.State.GetColorCode(),
                
                // Calculated properties
                HasSupervisor = project.HasSupervisor,
                HasCoordinates = project.HasCoordinates,
                HasImage = project.HasImage,
                IsActive = project.State.IsActive,
                IsCompleted = project.State.IsCompleted,
                CanBeDeleted = project.CanBeDeleted(),
                IsReadyToStart = project.IsReadyToStart(),
                IsOverdue = project.IsOverdue(),
                DaysUntilStart = project.DaysUntilStart(),
                
                // Summary and metadata
                ProjectSummary = project.GetProjectSummary(),
                DescriptionSummary = project.Description.GetSummary(150),
                
                // Audit fields
                CreatedAt = project.CreatedDate,
                UpdatedAt = project.UpdatedDate,
                
                // Validation
                ValidationErrors = project.GetValidationErrors(),
                IsValid = project.IsValid()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transform project {ProjectId} to resource", project.Id);
            throw;
        }
    }

    /// <summary>
    /// Transform Project entity to ProjectSummaryResource for lists
    /// </summary>
    public async Task<ProjectSummaryResource> ToSummaryResourceFromEntityAsync(Project project)
    {
        try
        {
            // Get basic user information (cached or simplified)
            var manager = await _userContextService.FindByIdAsync(project.ManagerId);
            
            UserDto? supervisor = null;
            if (project.SupervisorId.HasValue)
            {
                supervisor = await _userContextService.FindByIdAsync(project.SupervisorId.Value);
            }

            // Generate thumbnail if image exists
            string? thumbnailUrl = null;
            if (!string.IsNullOrEmpty(project.ImageUrl))
            {
                try
                {
                    var publicId = ExtractPublicIdFromUrl(project.ImageUrl);
                    if (!string.IsNullOrEmpty(publicId))
                    {
                        thumbnailUrl = _cloudinaryService.GenerateThumbnailUrl(publicId, 800);
                    }
                }
                catch
                {
                    // Ignore thumbnail generation errors for summary
                }
            }

            return new ProjectSummaryResource
            {
                Id = project.Id,
                Name = project.ProjectName,
                DescriptionSummary = project.Description.GetSummary(100),
                ThumbnailUrl = thumbnailUrl,
                ManagerName = manager?.FullName ?? "Unknown",
                ShortLocation = project.Location.GetShortLocation(30),
                StartDate = project.StartDate,
                SupervisorName = supervisor?.FullName,
                State = project.ProjectState,
                StateDisplayName = project.State.GetDisplayName(),
                StateColor = project.State.GetColorCode(),
                IsActive = project.State.IsActive,
                IsOverdue = project.IsOverdue(),
                DaysUntilStart = project.DaysUntilStart(),
                CreatedAt = project.CreatedDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transform project {ProjectId} to summary resource", project.Id);
            throw;
        }
    }

    /// <summary>
    /// Transform list of projects to paginated resource
    /// </summary>
    public async Task<ProjectPagedResource> ToPagedResourceFromEntitiesAsync(
        List<Project> projects, 
        int totalCount, 
        int pageNumber, 
        int pageSize)
    {
        var summaryTasks = projects.Select(ToSummaryResourceFromEntityAsync);
        var projectSummaries = await Task.WhenAll(summaryTasks);
        
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new ProjectPagedResource
        {
            Projects = projectSummaries.ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = pageNumber < totalPages,
            HasPreviousPage = pageNumber > 1
        };
    }

    /// <summary>
    /// Extract public ID from Cloudinary URL for thumbnail generation
    /// </summary>
    private static string ExtractPublicIdFromUrl(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
                return string.Empty;

            // Simple extraction for buildtruck/projects/projectImages/ structure
            var uri = new Uri(imageUrl);
            var pathSegments = uri.AbsolutePath.Split('/');
            
            // Find buildtruck segment and build public ID
            var buildtruckIndex = Array.FindIndex(pathSegments, s => s == "buildtruck");
            if (buildtruckIndex != -1 && buildtruckIndex < pathSegments.Length - 2)
            {
                var relevantSegments = pathSegments[(buildtruckIndex)..];
                var lastSegment = relevantSegments[^1];
                var lastSegmentWithoutExt = Path.GetFileNameWithoutExtension(lastSegment);
                relevantSegments[^1] = lastSegmentWithoutExt;
                
                return string.Join("/", relevantSegments);
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}