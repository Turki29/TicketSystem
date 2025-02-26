using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystem.Data;
using TicketSystem.Models;
using TicketSystem.Models.ViewModels;
using Utility;

namespace TicketSystem.Areas.Home.Controllers
{
    [Area("Home")]
    [Authorize(Roles = StaticData.Role_System_Admin)]
    public class SectionController : Controller
    {


        private readonly ApplicationDbContext _db;
        public SectionController(ApplicationDbContext db)
        {

            _db = db;
        }
        public IActionResult Index(int section)
        {
            if (section == 0 || section == null)
            {
                return NotFound();
            }

            List<ApplicationUserVM> ApplicationUserVM = _db.UserSections.Include(u => u.Role).Include(u => u.User)
                .Where(u => u.Role.Name == StaticData.Role_Section_Admin || u.Role.Name == StaticData.Role_Technician)
                .Select(u => new ApplicationUserVM { Id = u.UserId, Email = u.User.Email, RoleName = u.Role.Name })
                .ToList();

            ViewData["role"] = StaticData.Role_System_Admin;
            ViewData["sectionName"] = _db.Sections.FirstOrDefault(u => u.Id == section).Name;
            ViewData["section"] = section;

            ApplicationUserVM = ApplicationUserVM.OrderBy(u => u.RoleName).ToList();

            return View(ApplicationUserVM);

        }

        public IActionResult AddUser()
        {
            ViewData["role"] = StaticData.Role_System_Admin;
            return View();
        }
        public IActionResult PartialGetSectionRoles()
        {

            List<IdentityRole> roles = _db.Roles
                .Where(u => u.Name != StaticData.Role_User && u.Name != StaticData.Role_System_Admin)
                .ToList();

            return PartialView("_GetRoles", roles);

        }

        public IActionResult PartialGetUsers(int section)
        {

            List<ApplicationUserVM> roleid = _db.Roles
          .Where(u => u.Name == StaticData.Role_Section_Admin || u.Name == StaticData.Role_Technician)
          .Join(_db.UserRoles
          , roleId => roleId.Id,
          userroles => userroles.RoleId
          ,
          (role, userRole) => new { role.Name, userRole.UserId }
          )
          .Join(_db.Users,
          userRoleName => userRoleName.UserId,
          users => users.Id,
          (userRoleName, user) => new ApplicationUserVM
          {
              Email = user.Email,
              Id = user.Id
          }
          ).ToList();
            ;




            return PartialView("_GetUsers", roleid);
        }

        [HttpPost]
        public IActionResult AddUser(UserSections usersection)
        {
            UserSections usersectionChecker;

            //يتأكد أن القسم موجود
            Section section = _db.Sections.FirstOrDefault(u => u.Id == usersection.SectionId);
            if (section == null) return Redirect("/Home/Home/Error");

            //يتأكد أن المستخدم موجود
            IdentityUser user = _db.Users.FirstOrDefault(u => u.Id == usersection.UserId);
            if (user == null) return Redirect("/Home/Home/Error");

            //يتأكد أن الصلاحية ليست متكررة
            usersectionChecker = _db.UserSections.FirstOrDefault(u => u.UserId == usersection.UserId && u.SectionId == usersection.SectionId);
            if (usersectionChecker != null) return Redirect("/Home/Home/Error");

            // يتأكد أن المستخدم ليس عميلا
            var customerRoleId = _db.Roles.FirstOrDefault(u => u.Name == StaticData.Role_User).Id;
            bool isCustomer = _db.UserRoles.FirstOrDefault(u => u.RoleId == customerRoleId && u.UserId == usersection.UserId) != null;
            if (isCustomer) return Redirect("/Home/Home/Error");

            string roleName = _db.Roles.FirstOrDefault(u => u.Id == usersection.RoleId).Name;
            bool isValidRole = roleName != null && roleName != StaticData.Role_User && roleName != StaticData.Role_System_Admin;
            if (!isValidRole) return Redirect("/Home/Home/Error");


            _db.UserSections.Add(usersection);
            _db.SaveChanges();


            return RedirectToAction(nameof(Index), new { section = usersection.SectionId });
        }
        [HttpDelete]
        public IActionResult DeleteUserFromSection(string userId, int sectionId)
        {
            // مفروض يصير تصنيف خاص ويستقبل البيانات من الbody
            UserSections usersection = _db.UserSections.Include(u => u.Role)
                .FirstOrDefault(u => u.UserId == userId && u.SectionId == sectionId);

            if (usersection == null) return NotFound();

            if (usersection.Role.Name == StaticData.Role_Technician)
            {
                List<Ticket> ticketList = _db.Tickets
                    .Where(u => u.TechnicalIdentityUserId == userId)
                    .ToList();
                int size = ticketList.Count();
                for (int i = 0; i < size; i++)
                {
                    ticketList[i].TechnicalIdentityUserId = null;
                }

            }



            _db.UserSections.Remove(usersection);
            _db.SaveChanges();
            TempData["success"] = "حذف المستخدم من القسم";
            return Ok();
        }

        public bool IsSectionAdmin(string userId)
        {

            string roleId = _db.Roles.FirstOrDefault(u => u.Name == StaticData.Role_Section_Admin).Id;

            bool isSectionAdmin = _db.UserRoles.FirstOrDefault(u => u.UserId == userId && u.RoleId == roleId) != null;

            return isSectionAdmin;




        }


    }
}

