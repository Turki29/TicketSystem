namespace Utility
{
    public static class StaticData
    {

        public const string Role_System_Admin = "System_Admin";
        public const string Role_Section_Admin = "Super";
        public const string Role_Technician = "Tech";
        public const string Role_User = "User";


        public static string GetArabicRole(string role)
        {
            switch (role)
            {
                case Role_System_Admin:
                    return "مدير النظام";
                case Role_Section_Admin:
                    return "مدير القسم";
                case Role_Technician:
                    return "تقني";
                case Role_User:
                    return "مستخدم";
                default:
                    return "";
            }
        }

        public static string GetCountable(int count, string countableMofrad, string countableMuthanna, string countableJame, string zeroMessage = "الآن", string NotFoundMessage = "ليس عددا")

        {

            if (count == 0) return zeroMessage;
            else if (count
        < 0) return "لا يمكن أن يكون العدد سالبا";
            else if (count == 1) return countableMofrad;
            else if (count == 2) return countableMuthanna;
            else if (count >= 3 && count <= 10) return count + " " + countableJame;
            else if (count > 10) return count + " " + countableMofrad;

            return "ليس عددا";
        }

    }
}
