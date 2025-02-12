using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TicketSystem.Data;
using Models;

namespace TicketSystem.ClaimsFactory
{
    public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<IdentityUser>
    {
        private readonly ApplicationDbContext _context;

        public ApplicationUserClaimsPrincipalFactory(UserManager<IdentityUser> userManager,
            IOptions<IdentityOptions> optionsAccessor,
            ApplicationDbContext context)
            : base(userManager, optionsAccessor)
        {
            _context = context;
        }

        public override async Task<ClaimsPrincipal> CreateAsync(IdentityUser user)
        {
            var principal = await base.CreateAsync(user);
            var userPermissions = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == user.Id);

            if (userPermissions != null)
            {
                var permissionsClaim = new Claim("Permissions", ((int)userPermissions.Permissions).ToString());
                ((ClaimsIdentity)principal.Identity).AddClaim(permissionsClaim);
            }

            return principal;
        }
    }
}
