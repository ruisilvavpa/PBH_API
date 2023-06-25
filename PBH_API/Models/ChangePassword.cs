namespace PBH_API.Models
{
    public class ChangePassword
    {
        public String oldPassword { get; set; }
        public String newPassword { get; set; }
        public String confirmPassword { get; set; }
    }
}
