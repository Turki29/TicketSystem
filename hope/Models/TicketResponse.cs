using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSystem.Models
{
    public class TicketResponse
    {

        // علاقة الردود والتذاكر
        public int Id { get; set; }
        public int TicketId { get; set; }
        [ForeignKey("TicketId")]
        [ValidateNever]
        public Ticket Ticket { get; set; }

        // علاقة الردود والمستخدمين
        public string SenderId { get; set; }
        [ForeignKey("SenderId")]
        [ValidateNever]
        public IdentityUser Sender { get; set; }


        // محتويات الرد
        public string SenderName { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime DateSent { get; set; }
        

    }
}
