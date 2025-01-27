using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using TicketSystem.Data;
using TicketSystem.Models;
using TicketSystem.Models.ViewModels;
using Utility;
using static System.Net.Mime.MediaTypeNames;

namespace TicketSystem.Areas.Home.Controllers
{
    [Area("Home")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {

            

            
            




            IEnumerable<Section> sectionsList;

            if (!(User.IsUser() || User.IsSystemAdmin()))
            {

                //List <UserSections> userSection 
                sectionsList = _db.UserSections
                    .Where(u => u.UserId == User.GetUserId())
                    .Join(_db.Sections,
                        usersection => usersection.SectionId,
                        section => section.Id,
                        (usersection, section) => new Section
                        { 
                            Id = section.Id,
                            Name = section.Name
                        });




                return View(sectionsList);

            }

            sectionsList = _db.Sections.Where(u => u.Id !=4);

            return View(sectionsList);
        }

        public IActionResult AddSection(string sectionName)
        {
            if(string.IsNullOrEmpty(sectionName)) return RedirectToAction(nameof(Index));

            Section section = new Section();
            section.Name = sectionName;
            _db.Sections.Add(section);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult TicketsView(int section,string? status = "new", string? filter = "")
        {


            if(!(User.IsUser() || User.IsSystemAdmin()))
            {
                if (section == 4 || section < 1 || !IsCurrentUserInSection(section)) return NotFound();
            }

            IEnumerable<Ticket> ticketList = Enumerable.Empty<Ticket>(); ;


            

            ViewData["section"] = section.ToString();
            ViewData["sectionName"] = _db.Sections.FirstOrDefault(u => u.Id == section);

           


            // SECTION ADMIN AND SYSTEM ADMIN VIEW
            if (User.IsSectionAdmin() || User.IsSystemAdmin())
            {
                ticketList = _db.Tickets.Include(u => u.TechnicalApplicationUser)
                  .Where(u => u.SectionId == section && u.Status.ToLower() == status.ToLower() && u.IsDeleted == false);

                return View(ticketList);
            }

            // TECH VIEW
            if(User.IsTechnician())
            {
                GetTechTickets(ref ticketList,section,status,filter);
                // تجيب تذاكر التقني و التذاكر اللي ما مسكها أحد
                return View(ticketList);
            }

            ticketList = _db.Tickets.Include(u => u.TechnicalApplicationUser)
                    .Where(u => u.SenderApplicationUserId == User.GetUserId() && u.SectionId == section && u.Status.ToLower() == status.ToLower() && u.IsDeleted == false);
            return View(ticketList);

        }

        public void GetTechTickets(ref IEnumerable<Ticket> ticketList ,int section,string status,string filter)
        {


            if(string.IsNullOrEmpty(filter))
            {
                ticketList = _db.Tickets.Include(u => u.TechnicalApplicationUser)
                                  .Where(u =>
                                  u.SectionId == section 
                                  &&
                                  (u.TechnicalApplicationUserId == User.GetUserId() || u.TechnicalApplicationUserId == null)
                                  &&
                                  u.Status.ToLower() == status.ToLower()
                                  &&
                                  u.IsDeleted == false
                                  )
                                  .ToList();
            }
            else if(filter.ToLower() == "notassigned")
            {
                ticketList =  _db.Tickets.Include(u => u.TechnicalApplicationUser)
                                  .Where(u =>
                                   u.SectionId == section
                                  &&
                                  ( u.TechnicalApplicationUserId == null) 
                                  && 
                                  u.Status.ToLower() == status.ToLower() 
                                  && 
                                  u.IsDeleted == false)
                                  .ToList();
            }
            else if (filter.ToLower() == "techtickets")
            {
                ticketList = _db.Tickets.Include(u => u.TechnicalApplicationUser)
                      .Where(u =>
                       u.SectionId == section
                       &&
                      (u.TechnicalApplicationUserId == User.GetUserId())
                      && 
                      u.Status.ToLower() == status.ToLower()
                      && 
                      u.IsDeleted == false)
                      .ToList();
            }


         

        }

        public IActionResult Insert(int section)
        {

            Ticket ticket = new Ticket();
            ticket.SectionId = section;

            return View(ticket);
        }
        public IActionResult PartialSections(string selectedSection = "")
        {
            List<Section> sectionList = _db.Sections.Where(u => u.Id != 4).ToList();
            ViewData["selectedSection"] = selectedSection;
            return PartialView("_Sections", sectionList);
        }

        [HttpPost]
        public IActionResult Insert(Ticket ticket, IFormFile? file)
        {
            

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ticket.SenderApplicationUserId = userId;

            if(file != null)
            {

                try
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);




                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "PrivateFiles", "Ticket_Attachment_Images");
                    Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    ticket.TicketImageAttachmentPath = fileName;
                }
                catch(Exception e)
                {

                }

            }



            _db.Tickets.Add(ticket);
            _db.SaveChanges();
            TempData["success"] = "فُتحت التذكرة";
            return RedirectToAction(nameof(TicketsView), new { section = ticket.SectionId });
        }


        public IActionResult Details(int Id)
        {

            

            Ticket ticket = _db.Tickets
               .Include(u => u.SenderApplicationUser)
               .Include(u => u.Section)
               .FirstOrDefault(u => u.Id == Id);


            // IS TICKET NULL?
            if (ticket == null) return NotFound();

            ViewData["MessagesOfTicket"] = _db.TicketResponses.Where(u => u.TicketId == ticket.Id).ToList();



            // ADMIN
            if ((User.IsSectionAdmin() && IsCurrentUserInSection(ticket.SectionId)) || User.IsSystemAdmin())
            {
                return View(ticket);
            }
            
            // TECH
            if (User.IsTechnician())
            {
                if(IsCurrentUserInSection(ticket.SectionId)
                    &&
                   (
                    ticket.TechnicalApplicationUserId == User.GetUserId() 
                    || 
                    ticket.TechnicalApplicationUserId == null)
                    // نهاية الشرط
                  )
                return View(ticket);
            }

            // USER
            if (User.GetUserId() == ticket.SenderApplicationUserId) return View(ticket); 
            
            
            
            return NotFound();


        }


        [HttpPost]
        [Authorize(Roles = StaticData.Role_Section_Admin +","+StaticData.Role_Technician+ "," + StaticData.Role_System_Admin)]
        public IActionResult Details(Ticket ticket)
        {

           

           


            Ticket dbTicket = _db.Tickets.FirstOrDefault(u => u.Id == ticket.Id);

            //يتأكد إذا المستخدم عنده صلاحية على التذكرة أو لا
            //if (!IsCurrentUserInSection(ticket.SectionId)) return Redirect("/Home/Error");



            //تحديث الحالة
            if(User.IsSectionAdmin() || User.GetUserId() == ticket.TechnicalApplicationUserId || User.IsSystemAdmin())
            {
                if (ticket.Status.ToLower() == "closed")
                {
                    dbTicket.ClosedAt = DateTime.Now;
                    dbTicket.Status = ticket.Status;
                }
                else if (ticket.Status.ToLower() == "new") dbTicket.Status = ticket.Status;
                else return Redirect("/Home/Home/Error");  
                
            }

            int originalSection = dbTicket.SectionId;
            if (User.IsSectionAdmin() || User.IsSystemAdmin())
            {
                // تغيير القائم على التذكرة إذا كان التقني في القسم
                if(IsThisUserIdInSection(ticket.TechnicalApplicationUserId, ticket.SectionId)) dbTicket.TechnicalApplicationUserId = ticket.TechnicalApplicationUserId;

                // وزن التذكرة
               
                dbTicket.RelativeWeight = ticket.RelativeWeight;

                // قسم التذكرة
                dbTicket.SectionId = ticket.SectionId;

            }

            if(User.GetUserId() == dbTicket.TechnicalApplicationUserId || User.IsSystemAdmin())
            {

                if(!string.IsNullOrEmpty(ticket.TechnicalResponse))
                {
                    //dbTicket.TechnicalResponse = ticket.TechnicalResponse;
                    //dbTicket.TechResponseAt = DateTime.Now;
                }

                

            }

            _db.SaveChanges();
            TempData["success"] = "حُدّثت التذكرة";

            return RedirectToAction(nameof(TicketsView), new { section = originalSection });



        }


        [Authorize(Roles = StaticData.Role_Section_Admin + "," + StaticData.Role_System_Admin)]
        public IActionResult PartialAssignedTech(string assignedTicketTech, string section ="")
        {
            

            TechnicalsVM technicalsVM = new TechnicalsVM() 
            {
            
                SelectedTech = assignedTicketTech,
                

            };

            var Role = _db.Roles.FirstOrDefault(u => u.Name == StaticData.Role_Technician);
            string RoleId = Role.Id;

            var users = _db.UserRoles
                            .Where(u => u.RoleId == RoleId)
                            .Join(
                                _db.UserSections,
                                userRole => userRole.UserId,
                                userSection => userSection.UserId,
                                (userRole, userSection) => new { userSection.UserId, userSection.SectionId }
                            )
                            .ToList();

            
            if (!string.IsNullOrEmpty(section))
            {
                int sectionId = Convert.ToInt32(section);
                users = users.Where(u => u.SectionId == sectionId).ToList();
            }

            ApplicationUser ApplicationUser;
            technicalsVM.Technicians = new List<SelectListItem>();


            foreach (var user in users)
            {
                ApplicationUser = _db.Users.FirstOrDefault(u => u.Id == user.UserId);
                
                if (ApplicationUser != null)
                {
                    technicalsVM.Technicians.Add(new SelectListItem
                    {
                        Text = ApplicationUser.UserName, // Display name
                        Value = ApplicationUser.Id       // User ID as the value // ** ليش كذا خلهم تقني لا تخليهم عنصر
                    });
                }
            }


            return PartialView("_UserRoles", technicalsVM);
        }

        public IActionResult GetImage(string imageName)
        {
            if (string.IsNullOrEmpty(imageName)) return NotFound();
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "PrivateFiles", "Ticket_Attachment_Images", imageName);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, "image/png");


        }

        
        
        
        [HttpPost]
        [Authorize(Roles = StaticData.Role_Section_Admin + "," + StaticData.Role_User + "," + StaticData.Role_System_Admin)]
        public IActionResult Delete(int Id)
        {

            

            Ticket ticket = _db.Tickets.FirstOrDefault(u => u.Id == Id);

            if (ticket == null || (User.IsUser() && ticket.SenderApplicationUserId != User.GetUserId()))
            {
                return NotFound();
            }

            
            
            _db.Tickets.Remove(ticket);
            _db.SaveChanges();
            
           
            
           
            TempData["success"] = "حُذفت التذكرة";
            return Ok();
        
        }




        [Authorize(Roles = StaticData.Role_Section_Admin + "," + StaticData.Role_System_Admin)]
        public IActionResult Assign(int id, string techId)
        {
            
            

            Ticket ticket = _db.Tickets.FirstOrDefault(u => u.Id == id);

            ApplicationUser user = _db.Users.FirstOrDefault(u => u.Id == techId);


            if(user == null) return Redirect("/Home/Home/Error");

            bool TechIsntInSection= _db.UserSections.FirstOrDefault(u => u.UserId == user.Id && u.SectionId == ticket.SectionId) == null;
            if (TechIsntInSection) return Redirect("/Home/Home/Error");

            ticket.TechnicalApplicationUserId = techId;
            _db.SaveChanges();

            return RedirectToAction(nameof(Index));


        }


        public bool IsCurrentUserInSection(int sectionId)
        {
            if(User.IsSystemAdmin())
            {
                return true;
            }
            UserSections queryUserSection = _db.UserSections.FirstOrDefault(u => u.UserId == User.GetUserId() && u.SectionId == sectionId);

            if (queryUserSection == null) { return false; }
            else { return true; }
        }

        public bool IsThisUserIdInSection(string userId,int sectionId)
        {
            UserSections queryUserSection = _db.UserSections.FirstOrDefault(u => u.UserId == userId && u.SectionId == sectionId);

            if (queryUserSection == null) { return false; }
            else { return true; }
        }

        public IActionResult SendMessage(int TicketId, string Message)
        {

            

            if (string.IsNullOrEmpty(Message)) return NotFound();

            TicketResponse ticketResponse = new TicketResponse();

            
            
            Ticket ticket = _db.Tickets.FirstOrDefault(u => u.Id == TicketId);
            if (ticket == null) return NotFound();

            if(User.GetUserId() != ticket.TechnicalApplicationUserId && User.GetUserId() != ticket.SenderApplicationUserId) return NotFound();
            
            if(User.GetUserId() == ticket.TechnicalApplicationUserId)
            {
                ticket.UnresponsedMessage = false;
            }
            else
            {
                ticket.UnresponsedMessage = true;
            }

            ticketResponse.SenderName = User.GetUserEmail();
            ticketResponse.TicketId = ticket.Id;
            ticketResponse.SenderId = User.GetUserId();
            ticketResponse.Message = Message;
            ticketResponse.DateSent = DateTime.Now;
            

            _db.TicketResponses.Add(ticketResponse);
            _db.SaveChanges();
            TempData["success"] = "أرسلت رسالتك";
            
            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
