﻿using Microsoft.AspNetCore.Identity;

namespace TicketSystem.Models.ViewModels
{
    public class IdentityUserVM
    {
        public string Id { get; set; } 
        public string Email { get; set; }
        public string RoleName { get; set; }
        

    }
}
