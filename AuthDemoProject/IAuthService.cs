using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthDemoProject
{
    public interface IAuthService
    {
        Task<bool> Register(UserRegisteration user);
        Task<List<IdentityUser>> GetAllRegisteredUsers();
        Task<dynamic> GetAllRoles();
        Task<string> Login(UserLogin userLogin, HttpContext httpContext);
        Task<string> GenerateTokenString(UserLogin userLogin,List<string> roles);
        Task<dynamic> GetCurrentRole(HttpContext httpContext);
        Task<string> UpdateUser(string id, Dictionary<string, string> updates);
        
    }
}
