using System.Reflection.Metadata.Ecma335;

namespace TicketSystem.Models.ViewModels
{
    public class TechnicalVM
    {

        public string Id { get; set; }
        public string Email { get; set; }
        public int TasksCount { get; set; }

        public IEnumerable<string> Sections { get; set; }




    }
}
