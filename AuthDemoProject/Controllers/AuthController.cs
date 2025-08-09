using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthDemoProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<bool> Register(UserRegisteration user)
        {
            return await _authService.Register(user);
        }
        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<string> Login(UserLogin user)
        {
            HttpContext httpContext = HttpContext;
            return await _authService.Login(user, httpContext);
        }
        [HttpGet("GetAllRegisteredUsers")]
        //[Authorize(Roles ="admin")]
        public async Task<List<IdentityUser>> GetAllRegisteredUsers()
        {
            return await _authService.GetAllRegisteredUsers();
        }
        [HttpGet("GetAllRoles")]
        //[Authorize(Roles = "admin")]
        public async Task<dynamic> GetAllRoles()
        {
            return await _authService.GetAllRoles();
        }       

        [HttpGet("GetData")]
        //[Authorize("memberPolicy")]             //This is Policy Based Authorization, here 'memberPolicy' is the name of policy.
        [Authorize]
        public async Task<string> GetData()
        {
            return "Hi Can you access My?";
        }
        [HttpGet("GetCurrentRole")]
        //[Authorize("AccessForAllPolicy")]
        public Task<dynamic> GetCurrentRole()
        {
            var httpContext = HttpContext;
            return _authService.GetCurrentRole(httpContext);
        }
        [HttpPut("UpdateUser")]
        public Task<string> UpdateUser(string id, Dictionary<string,string> updates)
        {
            return _authService.UpdateUser(id, updates);        
        }
    }
}
