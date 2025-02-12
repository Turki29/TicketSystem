using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
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
        //public string RoleId { get; set; }
        //[ForeignKey("RoleId")]
        //[ValidateNever]
        //public IdentityRole Role { get; set; }

    }
}
