using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data;
using System.Reflection.Emit;
using Models;

namespace TicketSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options )
            : base(options)
        {
            

        }


        public DbSet<Ticket> Tickets { get; set; }
        [ValidateNever]
        public DbSet<Section> Sections { get; set; }
        [ValidateNever]
        public DbSet<UserSections> UserSections { get; set; }
        public DbSet<TicketResponse> TicketResponses { get; set; }

        public DbSet<UserPermission> UserPermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserSections>()
                .HasKey(us => new { us.UserId, us.SectionId });

            builder.Entity<UserPermission>()
                .HasKey(us => new { us.UserId, us.Permissions});

            builder.Entity<TicketResponse>()
            .HasOne(tr => tr.Sender)
            .WithMany()
            .HasForeignKey(tr => tr.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        }

    }
}
