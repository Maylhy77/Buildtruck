using BuildTruckDocumentationService.Documentation.Application.ACL.Services;
using BuildTruckDocumentationService.Users.Application.Internal.OutboundServices;

namespace BuildTruckDocumentationService.Documentation.Infrastructure.ACL;

/// <summary>
/// ACL Adapter to communicate with Users context
/// </summary>
public class UserContextService : IUserContextService
{
    private readonly IUserFacade _userFacade;

    public UserContextService(IUserFacade userFacade)
    {
        _userFacade = userFacade;
    }

    public async Task<bool> UserExistsAsync(int userId)
    {
        try
        {
            var user = await _userFacade.FindByIdAsync(userId);
            return user != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<string?> GetUserEmailAsync(int userId)
    {
        try
        {
            var user = await _userFacade.FindByIdAsync(userId);
            return user?.Email; // Usa la propiedad calculada que devuelve CorporateEmail.Address
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> UserHasPermissionAsync(int userId, string permission)
    {
        try
        {
            var user = await _userFacade.FindByIdAsync(userId);
            if (user == null) return false;
            
            // Implementar lógica de permisos basada en UserRole
            return user.Role.ToString().ToLower() == permission.ToLower() || 
                   user.Role.ToString() == "Admin"; // Admin tiene todos los permisos
        }
        catch (Exception)
        {
            return false;
        }
    }
}