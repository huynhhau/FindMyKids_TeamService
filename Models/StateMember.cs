namespace FindMyKids.TeamService.Models
{
    public class StateMember
    {
        public readonly static string Allow = "Kích hoạt";
        public readonly static string Deny = "Ngưng";
    }

    public class TypeMember
    {
        public readonly static string Admin = "Admin";
        public readonly static string Customer = "Customer";
    }

    public class PageInfo
    {
        public readonly static int PerPage = 20;
    }
}