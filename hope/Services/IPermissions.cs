using System.Security.Claims;

namespace TicketSystem.Services
{
    public interface IPermissions
    {

        bool IsSectionAdmin(ClaimsPrincipal User, int sectionId);
        bool IsTechnical(ClaimsPrincipal User, int sectionId);
        bool IsUser(ClaimsPrincipal User, int sectionId);

    }
}
