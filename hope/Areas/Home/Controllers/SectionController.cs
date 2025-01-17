using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            if(section == 0 || section == null)
            {
                return NotFound();
            }

            List<IdentityUserVM> idUserVM = Enumerable.Empty<IdentityUserVM>().ToList();

            List<UserSections> users = _db.UserSections.Where(u => u.SectionId == section).ToList();

            var roleNameUserId = _db.Roles
                .Where(u => u.Name == StaticData.Role_Section_Admin || u.Name == StaticData.Role_Technician)
                .Join(_db.UserRoles
                , roles => roles.Id
                , userRoles => userRoles.RoleId,
                (roles, userRoles) => new { RoleName = roles.Name, userRoles.UserId }
                )
                .ToList();


            ViewData["sectionName"] = _db.Sections.FirstOrDefault(u => u.Id == section).Name;
            ViewData["section"] = section;
            foreach (UserSections userSection in users)
            {
                IdentityUserVM userVM = new IdentityUserVM();
                userVM.Id = userSection.UserId;
                userVM.Email = _db.Users.FirstOrDefault(u => u.Id == userSection.UserId).Email;
                userVM.RoleName = roleNameUserId.FirstOrDefault(u => u.UserId == userSection.UserId).RoleName;
                idUserVM.Add(userVM);


            }

            idUserVM = idUserVM.OrderBy(u => u.RoleName).ToList();

            return View(idUserVM);

        }

        public IActionResult AddUser()
        {
            return View();
        }
        public IActionResult PartialGetUsers(int section)
        {

            List<IdentityUserVM> roleid = _db.Roles
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
                (userRoleName, user) => new IdentityUserVM
                {
                    Email = user.Email,
                    Id = user.Id
                }
                ).ToList();
                ;
                
                


            return PartialView("_GetUsers",  roleid);
        }

        [HttpPost]
        public IActionResult AddUser(UserSections usersection)
        {
            UserSections usersectionChecker;
           


            Section section = _db.Sections.FirstOrDefault(u => u.Id == usersection.SectionId);
            if (section == null) return Redirect("/Home/Home/Error");

            IdentityUser user = _db.Users.FirstOrDefault(u => u.Id == usersection.UserId);
            if (user == null) return Redirect("/Home/Home/Error");

             usersectionChecker = _db.UserSections.FirstOrDefault(u => u.UserId == usersection.UserId && u.SectionId == usersection.SectionId);
            if(usersectionChecker != null) return Redirect("/Home/Home/Error");


            var customerRoleId= _db.Roles.FirstOrDefault(u => u.Name == StaticData.Role_User).Id;
            bool isCustomer = _db.UserRoles.FirstOrDefault(u => u.RoleId == customerRoleId && u.UserId == usersection.UserId) != null;
            if (isCustomer) return Redirect("/Home/Home/Error");



            _db.UserSections.Add(usersection);
            
            _db.SaveChanges();


            return RedirectToAction(nameof(Index), new { section = usersection.SectionId});
        }
        [HttpDelete]
        public IActionResult DeleteUserFromSection(string userId, int sectionId)
        {
            // مفروض يصير تصنيف خاص ويستقبل البيانات من الbody
            var usersection = _db.UserSections.FirstOrDefault(u => u.UserId == userId && u.SectionId == sectionId);

            if (usersection == null) return NotFound();

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



//IEnumerable<Section> sectionsList;

//if (!(User.IsUser() || User.IsSystemAdmin()))
//{

//    //List <UserSections> userSection 
//    sectionsList = _db.UserSections
//        .Where(u => u.UserId == User.GetUserId())
//        .Join(_db.Sections,
//            usersection => usersection.SectionId,
//            section => section.Id,
//            (usersection, section) => new Section
//            {
//                Id = section.Id,
//                Name = section.Name
//            });




//    return View(sectionsList);

//}

//sectionsList = _db.Sections.Where(u => u.Id != 4);
