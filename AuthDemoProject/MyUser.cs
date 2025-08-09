using Microsoft.AspNetCore.Identity;

namespace AuthDemoProject
{
    public class MyUser : IdentityUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
    }
}
