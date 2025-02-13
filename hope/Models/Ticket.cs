using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSystem.Models
{
    public class Ticket
    {


        public int Id { get; set; }
        [Required(ErrorMessage = "فضلا املأ الحقل")]
        [DisplayName("وصف المشكلة")]

        public int SectionId { get; set; } = 4;
        //[ForeignKey("SectionId")]
        [ValidateNever]
        public Section Section { get; set; }
        public string ProblemDescription { get; set; }

        public string? TicketImageAttachmentPath { get; set; }

        [DisplayName("حالة التذكرة")]
        public string Status { get; set; } = "new";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ClosedAt { get; set; }

        public string? TechnicalResponse { get; set; }

        public DateTime? TechResponseAt { get; set; }


        public bool IsDeleted { get; set; } = false;

        public byte RelativeWeight { get; set; } = 1;


        // هل هناك رسالة لم يرد عليها؟
        public bool UnresponsedMessage { get; set; }

        /// التقني القائم عليها 
        public DateTime TicketAssignDate { get; set; }


        /// مرسل التذكرة
        [Required]
        public string SenderIdentityUserId { get; set; }
        [ForeignKey("SenderIdentityUserId")]
        [ValidateNever]
        public IdentityUser SendeIdentityUser { get; set; }

        public string? TechnicalIdentityUserId { get; set; }
        [ForeignKey("TechnicalIdentityUserId ")]
        [ValidateNever]
        public IdentityUser TechnicalIdentityUser { get; set; }

        



    }
}
