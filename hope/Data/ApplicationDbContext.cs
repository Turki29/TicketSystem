using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TicketSystem.Models;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserSections>()
                .HasKey(us => new { us.UserId, us.SectionId });


        }


    }
}
