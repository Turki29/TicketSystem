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
using TicketSystem.Migrations;
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

        // تظهر لك الأقسام المسموح لك ولوجها
        public IActionResult Index()
        {
            

            UserSections UserSection = _db.UserSections.Include(u => u.Role)
                .FirstOrDefault(u => u.UserId == User.GetUserId());


            IEnumerable<Section> sectionsList;

            if (!(UserSection ==null || User.IsSystemAdmin()))
            {

                
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



                ViewData["role"] = UserSection.Role.Name;
                return View(sectionsList);

            }

            sectionsList = _db.Sections.Where(u => u.Id !=4);
            if (UserSection == null) ViewData["role"] = StaticData.Role_User;
            else ViewData["role"] = StaticData.Role_System_Admin;
            return View(sectionsList);
        }

        public IActionResult AddSection(string sectionName)
        {

            if (string.IsNullOrEmpty(sectionName)) return RedirectToAction(nameof(Index));

            if (!(_db.UserSections.Include(u => u.Role).FirstOrDefault(u => u.UserId == User.GetUserId()).Role.Name == StaticData.Role_System_Admin))
            {
                return Redirect("/Home/Home/Error");
            }

            Section section = new Section();
            section.Name = sectionName;
            _db.Sections.Add(section);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // دالة تظهر لك التذاكر على حسب قسمك وصلاحيتك
        public IActionResult TicketsView(int section,string? status = "new", string? filter = "")
        {


            
            if (section > 3 || section < 1) return NotFound();

            IEnumerable<Ticket> ticketList = Enumerable.Empty<Ticket>(); ;


            ViewData["section"] = section.ToString();
            ViewData["sectionName"] = _db.Sections.FirstOrDefault(u => u.Id == section);
            if (User.IsSystemAdmin()) // SYSTEM ADMIN VIEW
            {
                ticketList = _db.Tickets.Include(u => u.TechnicalIdentityUser)
                      .Where(u => u.SectionId == section && u.Status.ToLower() == status.ToLower() && u.IsDeleted == false);
                ViewData["role"] = StaticData.Role_System_Admin;
                return View(ticketList);


            }

            UserSections UserSection = _db.UserSections.Include(u => u.Role).FirstOrDefault(u => u.UserId == User.GetUserId() && u.SectionId == section);

            
            
            if (UserSection == null) // USER VIEW
            {
                ticketList = _db.Tickets.Include(u => u.TechnicalIdentityUser)
                                     .Where(u => u.SenderIdentityUserId == User.GetUserId() && u.SectionId == section && u.Status.ToLower() == status.ToLower() && u.IsDeleted == false);
                ViewData["role"] = StaticData.Role_User;
                return View(ticketList);
            }


            switch(UserSection.Role.Name) 
            {

                
                case StaticData.Role_Section_Admin:

                    ticketList = _db.Tickets.Include(u => u.TechnicalIdentityUser)
                      .Where(u => u.SectionId == section && u.Status.ToLower() == status.ToLower() && u.IsDeleted == false);
                    ViewData["role"] = StaticData.Role_Section_Admin;
                    return View(ticketList);
                case StaticData.Role_Technician:
                default:
                    GetTechTickets(ref ticketList, section, status, filter);
                    // تجيب تذاكر التقني و التذاكر اللي ما مسكها أحد
                    ViewData["role"] = StaticData.Role_Technician;
                    return View(ticketList);
                
            }
        }

        //  TicketsView()هذه دالة تابعة لـ
        private void GetTechTickets(ref IEnumerable<Ticket> ticketList ,int section,string status,string filter)
        {


            if(string.IsNullOrEmpty(filter))
            {
                ticketList = _db.Tickets.Include(u => u.TechnicalIdentityUser)
                                  .Where(u =>
                                  u.SectionId == section 
                                  &&
                                  (u.TechnicalIdentityUserId == User.GetUserId() || u.TechnicalIdentityUserId == null)
                                  &&
                                  u.Status.ToLower() == status.ToLower()
                                  &&
                                  u.IsDeleted == false
                                  )
                                  .ToList();
            }
            else if(filter.ToLower() == "notassigned")
            {
                ticketList =  _db.Tickets.Include(u => u.TechnicalIdentityUser)
                                  .Where(u =>
                                   u.SectionId == section
                                  &&
                                  ( u.TechnicalIdentityUserId == null) 
                                  && 
                                  u.Status.ToLower() == status.ToLower() 
                                  && 
                                  u.IsDeleted == false)
                                  .ToList();
            }
            else if (filter.ToLower() == "techtickets")
            {
                ticketList = _db.Tickets.Include(u => u.TechnicalIdentityUser)
                      .Where(u =>
                       u.SectionId == section
                       &&
                      (u.TechnicalIdentityUserId == User.GetUserId())
                      && 
                      u.Status.ToLower() == status.ToLower()
                      && 
                      u.IsDeleted == false)
                      .ToList();
            }


         

        }

        public IActionResult Insert(int section)
        {
            UserSections userSections = _db.UserSections.Include(u => u.Role)
                .FirstOrDefault(u => u.UserId == User.GetUserId() && u.SectionId == section);
            //يتأكد إذا المتسخدم عميل
            if (userSections != null) return Redirect("/Home/Home/Error");

            // إذا المستخدم عميل
            Ticket ticket = new Ticket();
            ticket.SectionId = section;
            ViewData["role"] = StaticData.Role_User;
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
            

            
            ticket.SenderIdentityUserId = User.GetUserId();

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
               .Include(u => u.SenderIdentityUser)
               .Include(u => u.Section)
               .FirstOrDefault(u => u.Id == Id);

            

            // IS TICKET NULL?
            if (ticket == null) return Redirect("/Home/Home/Error");

            UserSections userSection = _db.UserSections.Include(u => u.Role)
                .FirstOrDefault(u => u.UserId == User.GetUserId() && u.SectionId == ticket.SectionId);


            
            
            if(User.IsSystemAdmin())
            {
                ViewData["MessagesOfTicket"] = _db.TicketResponses.Where(u => u.TicketId == ticket.Id).ToList();
                ViewData["role"] = StaticData.Role_System_Admin;
                return View(ticket);
            }

                // USER
            if (userSection == null )
            {
                if (ticket.SenderIdentityUserId == User.GetUserId())
                {
                    ViewData["MessagesOfTicket"] = _db.TicketResponses.Where(u => u.TicketId == ticket.Id).ToList();
                    ViewData["role"] = StaticData.Role_User;
                    return View(ticket);
                }
                else return Redirect("/Home/Home/Error");

            }
            
            ViewData["MessagesOfTicket"] = _db.TicketResponses.Where(u => u.TicketId == ticket.Id).ToList();
            ViewData["role"] = userSection.Role.Name;
            // Section admin and Technical VIEW
            return View(ticket);
       
        }


        [HttpPost]
        [Authorize(Roles = StaticData.Role_Section_Admin +","+StaticData.Role_Technician+ "," + StaticData.Role_System_Admin)]
        public IActionResult Details(Ticket ticket)
        {

            Ticket dbTicket = _db.Tickets.FirstOrDefault(u => u.Id == ticket.Id);

           UserSections userSection = _db.UserSections.FirstOrDefault(u => u.UserId == User.GetUserId() && u.SectionId == ticket.SectionId);
            
            //تحديث الحالة
            // ما نحتاج نتأكد إذا كان التقني بالقسم لأنه لا يتعين إلا وهو بالقسم
            if (User.IsSystemAdmin() ||userSection.IsSectionAdmin() || User.GetUserId() == ticket.TechnicalIdentityUserId)
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
            if (userSection.IsSectionAdmin() || User.IsSystemAdmin())
            {
                // تغيير القائم على التذكرة إذا كان التقني في القسم
                if(IsThisUserIdInSection(ticket.TechnicalIdentityUserId, ticket.SectionId)) dbTicket.TechnicalIdentityUserId = ticket.TechnicalIdentityUserId;

                // وزن التذكرة
               
                dbTicket.RelativeWeight = ticket.RelativeWeight;

                // قسم التذكرة
                if (ticket.SectionId < 1 || ticket.SectionId > 3) return Redirect("/Home/Home/Error");
                dbTicket.SectionId = ticket.SectionId;
            }

            // ما نحتاج نتأكد إذا كان التقني بالقسم لأنه لا يتعين إلا وهو بالقسم
            if(User.GetUserId() == dbTicket.TechnicalIdentityUserId || User.IsSystemAdmin())
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
        // لعرض التقنيين select يرجع لك وسم
        public IActionResult PartialAssignedTech(string assignedTicketTech, string section ="")
        {
            if(!User.IsSystemAdmin())
            {
                UserSections userSection = _db.UserSections.FirstOrDefault(u => u.UserId == User.GetUserId());

                if (userSection == null) return BadRequest(new { message = "You do not have the permission." });
            }

            TechnicalsVM technicalsVM = new TechnicalsVM() 
            {
                SelectedTech = assignedTicketTech,
            };

            if (string.IsNullOrEmpty(section)) return BadRequest(new { message = "Empty Section Number" });

            int sectionId = Convert.ToInt32(section);
            if (sectionId  == 4 || sectionId < 1) return BadRequest(new { message = "Invalid Section Number" });

            technicalsVM.Technicians = _db.UserSections
                .Include(u=> u.Role).Include(u => u.User)
                .Where(u=> u.Role.Name == StaticData.Role_Technician && u.SectionId == sectionId)
                .Select(u => new SelectListItem {Text = u.User.Email, Value = u.UserId })
                .ToList();

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

            if (ticket == null ) return BadRequest(new { message = "ticket not found"});

            if(User.IsSystemAdmin())
            {
                ticket.IsDeleted = true;
                _db.SaveChanges();
                TempData["success"] = "حُذفت التذكرة";
                return Ok();
            }

            UserSections userSection = _db.UserSections.FirstOrDefault(u => u.UserId == User.GetUserId() && u.SectionId == ticket.SectionId );

            
            if (userSection == null && User.GetUserId() != ticket.SenderIdentityUserId) return BadRequest(new { message = "ticket not found in your tickets" });

            if(userSection.IsTechnical()) return BadRequest(new { message = "you do not have the permission" });


            ticket.IsDeleted = true;
            _db.SaveChanges();
    
            TempData["success"] = "حُذفت التذكرة";
            return Ok();
        
        }




        [Authorize(Roles = StaticData.Role_Section_Admin + "," + StaticData.Role_System_Admin + "," + StaticData.Role_Technician)]
        public IActionResult Assign(int id, string techId)
        {
            
            

            Ticket ticket = _db.Tickets.FirstOrDefault(u => u.Id == id);

            IdentityUser user = _db.Users.FirstOrDefault(u => u.Id == techId);


            if(user == null) return Redirect("/Home/Home/Error");

            bool TechIsInSection= _db.UserSections.Include(u => u.Role).FirstOrDefault(u => u.UserId == user.Id && u.SectionId == ticket.SectionId && u.Role.Name == StaticData.Role_Technician) != null;
            if (!TechIsInSection) return Redirect("/Home/Home/Error");

            ticket.TechnicalIdentityUserId = techId;
            _db.SaveChanges();

            return RedirectToAction(nameof(Index));


        }


        private bool IsCurrentUserInSection(int sectionId)
        {
            if(User.IsSystemAdmin())
            {
                return true;
            }
            UserSections queryUserSection = _db.UserSections.FirstOrDefault(u => u.UserId == User.GetUserId() && u.SectionId == sectionId);

            return queryUserSection != null;
        }

        public bool IsThisUserIdInSection(string userId,int sectionId)
        {
            UserSections queryUserSection = _db.UserSections.FirstOrDefault(u => u.UserId == userId && u.SectionId == sectionId);

            if (queryUserSection == null) { return false; }
            else { return true; }
        }

        public IActionResult SendMessage(int TicketId, string Message, string isPrivate, IFormFile image)
        {



            if (string.IsNullOrEmpty(Message) && image == null) return BadRequest(new { Message = "الرسالة فارغة" });

           



            Ticket ticket = _db.Tickets.FirstOrDefault(u => u.Id == TicketId);
            if (ticket == null) return BadRequest(new { Message = "ما وجدنا التذكرة" });
            if (ticket.ClosedAt != null) return BadRequest(new { Message = "التذكرة مغلقة" });

            if (User.GetUserId() != ticket.TechnicalIdentityUserId && User.GetUserId() != ticket.SenderIdentityUserId) return NotFound();

            if (User.GetUserId() == ticket.TechnicalIdentityUserId)
            {
                ticket.UnresponsedMessage = false;
            }
            else
            {
                ticket.UnresponsedMessage = true;
            }

            TicketResponse ticketResponse = new TicketResponse();

            ticketResponse.SenderName = User.GetUserEmail();
            ticketResponse.TicketId = ticket.Id;
            ticketResponse.SenderId = User.GetUserId();
            ticketResponse.Message = Message;
            ticketResponse.DateSent = DateTime.Now;

            if (image != null)
            {

                try
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);




                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "PrivateFiles", "Ticket_Attachment_Images");
                    Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(fileStream);
                    }
                    ticketResponse.AttachmentPath = fileName;
                }
                catch (Exception e)
                {

                }

            }

            UserSections userSection = _db.UserSections.FirstOrDefault(u => u.UserId == User.GetUserId() && u.SectionId == ticket.SectionId);

            if (userSection == null)
            {
                ticketResponse.invisibleForCustomer = false;
            }
            else
            {
                ticketResponse.invisibleForCustomer = "on" == isPrivate ? true : false; // checkbox value is either on or off
            }


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
