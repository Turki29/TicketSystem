using Microsoft.AspNetCore.Identity;

namespace TicketSystem.Models
{
    public class ApplicationUserRole
    {
        public IdentityUser User { get; set; }
        public IdentityRole Role{ get; set; }
    }
}
