using Microsoft.CodeAnalysis.CSharp.Syntax;
using Utility;

namespace TicketSystem.Models
{
    public static class Extensions
    {

        public static bool IsSectionAdmin(this UserSections userSection)
        {
            
            try
            {
                return userSection.Role.Name == StaticData.Role_Section_Admin;
            }
            catch 
            {
                return false; 
            }
        }

    }
}
