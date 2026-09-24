
// Materials/Infrastructure/ACL/UserContextService.cs
using System;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Application.ACL.Services;
using BuildTruckMaterialsService.Projects.Application.Internal.OutboundServices;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BuildTruckMaterialsService.Materials.Infrastructure.ACL
{
    /// <summary>
    /// Implementation of user context service using HTTP context and Projects facade
    /// </summary>
    public class UserContextService : IUserContextService
    {
        private readonly IProjectFacade _projectFacade;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(
            IProjectFacade projectFacade,
            IHttpContextAccessor httpContextAccessor)
        {
            _projectFacade = projectFacade;
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User not authenticated or invalid user ID");
        }

        public string GetCurrentUserEmail()
        {
            var emailClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email);
            if (emailClaim != null)
            {
                return emailClaim.Value;
            }
            throw new UnauthorizedAccessException("User email not found in claims");
        }

        public async Task<bool> HasProjectAccessAsync(Guid projectId)
        {
            // This method should be deprecated in favor of the int version
            // For now, return false since we can't reliably convert Guid to int
            return false;
        }

        // Agregar método faltante para int projectId
        public async Task<bool> HasProjectAccessAsync(int projectId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                return await _projectFacade.UserHasAccessToProjectAsync(currentUserId, projectId);
            }
            catch
            {
                return false;
            }
        }
    }
}
