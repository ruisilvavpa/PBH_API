namespace PBH_API.Models
{
    public class UsersOut
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Email { get; set; }
        public String? Bio { get; set; }
        public int Type { get; set; }
        public String? ImagePath { get; set; }
    }
}
