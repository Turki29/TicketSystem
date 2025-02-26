using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class HelperFunctions
    {

        public static bool IsSystemAdmin(this ClaimsPrincipal user) // أضف اسم القسم
        {


            return user.IsInRole(StaticData.Role_System_Admin);
        }
        public static bool IsSectionAdmin(this ClaimsPrincipal user, object role) // أضف اسم القسم
        {

            
            return role.ToString() == StaticData.Role_Section_Admin || user.IsSystemAdmin();
            //return user.IsInRole(StaticData.Role_Section_Admin) || user.IsSystemAdmin();
        }

        


        // غير مستخدمة للآن
        public static bool IsSectionAdmin(this ClaimsPrincipal user,object role,string section)
        {

            // استخرج اسم القسم وطابقه مع القسم المعطى من الدالة
            return user.IsSectionAdmin(role)  ;
        }
        public static bool IsTechnician(this ClaimsPrincipal user, object role)
        {

            return role.ToString() == StaticData.Role_Technician ;
            //return user.IsInRole(StaticData.Role_Technician);
        }

        

        public static bool IsDepartment(this ClaimsPrincipal user, object role)
        {
            return user.IsSectionAdmin(role) || user.IsTechnician(role);
        }

       
        public static bool IsUser(this ClaimsPrincipal user,object role)
        {

            return role.ToString() == StaticData.Role_User ;
            //return user.IsInRole(StaticData.Role_User);
        }

        

        public static string GetUserId(this ClaimsPrincipal user)
        {
            string varies = user.FindFirstValue(ClaimTypes.NameIdentifier);

            return varies;
        }

        public static string GetUserName(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Name);
        }

        public static string GetUserEmail(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Email);
        }


    }
}


