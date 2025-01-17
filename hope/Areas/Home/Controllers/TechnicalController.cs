using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using TicketSystem.Data;
using TicketSystem.Models;
using TicketSystem.Models.ViewModels;
using Utility;

namespace TicketSystem.Areas.Home.Controllers
{
    [Area("Home")]
    [Authorize(Roles = StaticData.Role_System_Admin)]
    public class TechnicalController : Controller
    {

        private readonly ApplicationDbContext _db;

        public TechnicalController(ApplicationDbContext db)
        {
            _db = db;
        }


        public IActionResult Index()
        {




            List<TechnicalVM> technicals = new List<TechnicalVM>();

            var role = _db.Roles.FirstOrDefault(u => u.Name == StaticData.Role_Technician).Id;
            
            var usersIds = _db.UserRoles.Where(u => u.RoleId == role).ToList();



            TechnicalVM tech ;
            IdentityUser user;
            
            foreach (var userId in usersIds)
            {
                tech = new TechnicalVM();
                user = _db.Users.FirstOrDefault(u => u.Id == userId.UserId );
                
                tech.Id = user.Id;
                tech.Email = user.Email;
                tech.TasksCount = _db.Tickets.Where(u => u.TechnicalIdentityUserId == user.Id && u.IsDeleted == false && u.Status.ToLower() == "new").Count();

                technicals.Add(tech);


            }

            //IEnumerable<Section> sectionList = 


            return View(technicals);
        }


       
        public IActionResult AssignToSection()
        {



            return View();
        }
    }
}
