using Microsoft.EntityFrameworkCore;
using System.Security;
using System.Security.Claims;
using TicketSystem.Data;
using TicketSystem.Models;
using Utility;
using static System.Collections.Specialized.BitVector32;

namespace TicketSystem.Services
{
    public class Permissions : IPermissions
    {

        private readonly ApplicationDbContext _db;

        public Permissions(ApplicationDbContext db)
        {
            _db = db;
        }

        private bool HasThisPermission(ClaimsPrincipal User, int sectionId, string permission)
        {
            if (User.IsSystemAdmin()) return true;
            UserSections usersection = _db.UserSections.Include(u => u.Role)
                .FirstOrDefault(u => u.UserId == User.GetUserId() && u.SectionId == sectionId);

            if (usersection == null) return false;
            return usersection.Role.Name == permission;
        }

        public bool IsSectionAdmin(ClaimsPrincipal User, int sectionId)
        {
            return HasThisPermission(User, sectionId, StaticData.Role_Section_Admin);
        }

        public bool IsTechnical(ClaimsPrincipal User, int sectionId)
        {
            return HasThisPermission(User, sectionId, StaticData.Role_Technician);
        }

        public bool IsDepartment(ClaimsPrincipal User, int sectionId)
        {
            if (User.IsSystemAdmin()) return true;
            var hasAccess = _db.UserSections
                .Where(u => u.UserId == User.GetUserId() && u.SectionId == sectionId)
                .Select(u => u.Role.Name)
                .Any(roleName => roleName == StaticData.Role_Technician || roleName == StaticData.Role_Section_Admin);
            return hasAccess;
        }
        public bool IsUser(ClaimsPrincipal User, int sectionId)
        {
            if (User.IsSystemAdmin()) return false;
            UserSections usersection = _db.UserSections.Include(u => u.Role)
               .FirstOrDefault(u => u.UserId == User.GetUserId() && u.SectionId == sectionId);
            return usersection == null;
        }
    }
}
