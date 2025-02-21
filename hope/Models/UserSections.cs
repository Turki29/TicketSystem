using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Utility;

namespace TicketSystem.Models
{
    public class UserSections
    {

        
        
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public IdentityUser User { get; set; }

        public int SectionId { get; set; }
        [ForeignKey("SectionId")]
        [ValidateNever]
        public Section Section { get; set; }

        public string RoleId { get; set; } = "63c12b25-325a-444e-bf3f-1627d4a1b3aa";
        [ForeignKey("RoleId")]
        [ValidateNever]
        public IdentityRole Role { get; set; }



        public bool IsSectionAdmin()
        {
            return this.Role.Name == StaticData.Role_System_Admin;
        }

        public bool IsTechnical()
        {
            return this.Role.Name == StaticData.Role_Technician;
        }
        

    }
}
