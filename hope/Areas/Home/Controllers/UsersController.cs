using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Linq;
using TicketSystem.Data;
using TicketSystem.Models;
using TicketSystem.Models.ViewModels;
using Utility;

namespace TicketSystem.Areas.Home.Controllers
{
    [Area("Home")]
    [Authorize(Roles = StaticData.Role_System_Admin)]
    public class UsersController : Controller
    {

        private readonly ApplicationDbContext _db;

        public UsersController(ApplicationDbContext db)
        {
            _db = db;
        }


        public IActionResult Index()
        {




            List<DepartmentUserVM> departmentUsersVMList = new List<DepartmentUserVM>();

            var role = _db.Roles
                .Where(u => u.Name == StaticData.Role_Technician || u.Name == StaticData.Role_Section_Admin)
                .ToList();

            List<string> listOfIds= role.Select(u => u.Id).ToList();

            //UserRoles نوعه
            var userRoles = _db.UserRoles.Where(u => listOfIds.Contains(u.RoleId)).ToList();


            List<UserSections> userSection = _db.UserSections.Include(u => u.Section).ToList();


            DepartmentUserVM deptUserViewModel ;
            IdentityUser user;
            
            foreach (var userRole in userRoles) 
            {
                
                
                deptUserViewModel = new DepartmentUserVM();
                
                
                user = _db.Users.FirstOrDefault(u => u.Id == userRole.UserId );
                
                // معلومات عضو القسم
                deptUserViewModel.Id = user.Id;
                deptUserViewModel.Email = user.Email;

                // عدد المهام إذا كان تقني
                deptUserViewModel.TasksCount = _db.Tickets.Where(u => u.TechnicalIdentityUserId == user.Id && u.IsDeleted == false && u.Status.ToLower() == "new").Count();

                //الأقسام المنتسب إليها
                deptUserViewModel.Sections = userSection.Where(u => u.UserId == userRole.UserId).Select(u => u.Section.Name);

                //
                deptUserViewModel.Role = role.FirstOrDefault(u => u.Id == userRole.RoleId).Name;

                departmentUsersVMList.Add(deptUserViewModel);


            }

            departmentUsersVMList.OrderBy(u => u.Role);


            return View(departmentUsersVMList);
        }

     
       
        public IActionResult AssignToSection()
        {



            return View();
        }
    }
}
