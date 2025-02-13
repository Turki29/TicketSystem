using Microsoft.AspNetCore.Identity;

namespace TicketSystem.Models
{
    public class IdentityUserRole
    {
        public IdentityUser User { get; set; }
        public IdentityRole Role{ get; set; }
    }
}
