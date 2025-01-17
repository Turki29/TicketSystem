using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSystem.Models
{
    public class UserSections
    {

        
        
        public string UserId { get; set; }
        //// nav prop?
        
        public int SectionId { get; set; }

    }
}
