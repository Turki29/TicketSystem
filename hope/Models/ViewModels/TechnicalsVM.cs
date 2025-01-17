using Microsoft.AspNetCore.Mvc.Rendering;

namespace TicketSystem.Models.ViewModels
{

   
    public class TechnicalsVM
    {

      
        public string SelectedTech { get; set; }
        public List<SelectListItem> Technicians { get; set; }

    }
}
