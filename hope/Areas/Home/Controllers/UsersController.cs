using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;
using TicketSystem.Data;
using Models;
using Models.ViewModels;
using Utility;

namespace TicketSystem.Areas.Home.Controllers
{
    [Area("Home")]
    
    public class UsersController : Controller
    {

        private readonly ApplicationDbContext _db;

        public UsersController(ApplicationDbContext db)
        {
            _db = db;
        }


        public IActionResult Index()
        {

            if (!User.IsSystemAdmin())
            {
                return Redirect("/Identity/Account/AccessDenied");
            }


            List<DepartmentUserVM> departmentUsersVMList = new List<DepartmentUserVM>();

            var role = _db.Roles
                .Where(u => u.Name == StaticData.Role_Technician || u.Name == StaticData.Role_Section_Admin)
                .ToList();

            List<string> listOfIds = role.Select(u => u.Id).ToList();

            //UserRoles نوعه
            var userRoles = _db.UserRoles.Where(u => listOfIds.Contains(u.RoleId)).ToList();


            List<UserSections> userSection = _db.UserSections.Include(u => u.Section).ToList();


            DepartmentUserVM deptUserViewModel;
            IdentityUser user;

            foreach (var userRole in userRoles)
            {


                deptUserViewModel = new DepartmentUserVM();


                user = _db.Users.FirstOrDefault(u => u.Id == userRole.UserId);

                // معلومات عضو القسم
                deptUserViewModel.Id = user.Id;
                deptUserViewModel.Email = user.Email;

                // عدد المهام إذا كان تقني
                deptUserViewModel.TasksCount = _db.Tickets.Where(u => u.TechnicalIdentityUserId == user.Id && u.IsDeleted == false && u.Status.ToLower() == "new").Count();

                //الأقسام المنتسب إليها
                deptUserViewModel.Sections = userSection.Where(u => u.UserId == userRole.UserId).Select(u => u.Section.Name);

                //
                deptUserViewModel.Role = role.FirstOrDefault(u => u.Id == userRole.RoleId).Name;
                deptUserViewModel.RoleId = userRole.RoleId;

                departmentUsersVMList.Add(deptUserViewModel);


            }

            departmentUsersVMList.OrderBy(u => u.Role);


            return View(departmentUsersVMList);
        }

        public IActionResult RemoveFromSection(string Id, string Section)
        {
            if (!User.IsSystemAdmin())
            {
                return Redirect("/Identity/Account/AccessDenied");
            }
            UserSections userSections = _db.UserSections.Include(u => u.Section).FirstOrDefault(u => u.UserId == Id && u.Section.Name == Section);
            if (userSections == null) return NotFound();
            _db.UserSections.Remove(userSections);

            foreach (Ticket item in _db.Tickets.Include(u => u.Section).Where(u => u.TechnicalIdentityUserId == Id && u.Section.Name == Section))
            {
                item.TechnicalIdentityUserId = null;
            }

            _db.SaveChanges();
            return RedirectToAction(nameof(Index));

        }

        public IActionResult UpdateRole(string Id, string NewRoleId)
        {
            if (!User.IsSystemAdmin())
            {
                return Redirect("/Identity/Account/AccessDenied");
            }
            var userRoles = _db.UserRoles.FirstOrDefault(u => u.UserId == Id);

            if (userRoles == null) return NotFound();

            IdentityRole role = _db.Roles.FirstOrDefault(u => u.Id == NewRoleId); // الدور الجديد

            if (role == null) return NotFound();

            role = _db.Roles.FirstOrDefault(u => u.Id == userRoles.RoleId); // الدور القديم

            _db.UserRoles.Remove(userRoles);




            var newUserRole = new IdentityUserRole<string>
            {
                UserId = Id,
                RoleId = NewRoleId
            };

            if (role.Name == StaticData.Role_Technician)
            {
                foreach (Ticket item in _db.Tickets.Where(u => u.TechnicalIdentityUserId == Id))
                {
                    item.TechnicalIdentityUserId = null;
                }
            }

            _db.UserRoles.Add(newUserRole);
            _db.SaveChanges();

            return Ok();


        }
        public IActionResult Delete(string Id)
        {

            if (string.IsNullOrEmpty(Id)) return NotFound();

            IdentityUser user = _db.Users.FirstOrDefault(u => u.Id == Id);

            if (user == null) return NotFound();

            try
            {
                foreach (Ticket item in _db.Tickets.Where(u => u.TechnicalIdentityUserId == Id))
                {

                    item.TechnicalIdentityUserId = null;

                };

                _db.Users.Remove(user);
            }
            catch (Exception e)
            {


                return NotFound();

            }
            _db.SaveChanges();
            return Ok();





        }

        public IActionResult PartialRoles()
        {
            List<IdentityRole> Roles = _db.Roles.ToList();

            return PartialView("_GetRoles", Roles);

        }

       
    }
}
