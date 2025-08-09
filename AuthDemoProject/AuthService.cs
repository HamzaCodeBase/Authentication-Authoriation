using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;


namespace AuthDemoProject
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;

        public AuthService(UserManager<IdentityUser> userManager,RoleManager<IdentityRole> roleManager,IConfiguration config,ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            _context = context;
        }

        
        public async Task<bool> Register(UserRegisteration user)
        {
            var identityUser = new IdentityUser()
            {
                UserName = user.Username,
                Email = user.Email,
            };
            IdentityResult userResult = await _userManager.CreateAsync(identityUser,user.Password);
            if (!(await _roleManager.RoleExistsAsync(user.Role)))
            {
                IdentityRole identityRole = new IdentityRole(user.Role);
                await _roleManager.CreateAsync(identityRole);
            }
            IdentityResult result;
            try
            {

                result = await _userManager.AddToRoleAsync(identityUser, user.Role);

            }
            catch (Exception)
            {

                throw;
            }
            return result.Succeeded;

        }
        public async Task<dynamic> GetAllRoles()
        {
            //List<string> roles = _roleManager.Roles.Select(r=>r.Name).ToList();
            var totalRoles = _context.UserRoles.Join(_roleManager.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new
            {
                userRole = ur,
                role = r
            }).GroupBy(a => a.role.Name)
            .Select(g => new
            {
                Role = g.Key,
                Count = g.Count()
            });
            return totalRoles.ToList();
        }
        public async Task<List<IdentityUser>> GetAllRegisteredUsers()
        {
            return _userManager.Users.ToList();
        }
        public async Task<string> Login(UserLogin user,HttpContext httpContext)
        {    
            var identityUser = await _userManager.FindByNameAsync(user.UserName);
            if (identityUser is null)
                return "Wrong Username";
            var result =  await _userManager.CheckPasswordAsync(identityUser, user.Password);
            var roles = await _userManager.GetRolesAsync(identityUser);
            if (result)
            {
                var tokenString = await GenerateTokenString(user,roles.ToList());
                return tokenString;
            }
            return "Wrong Password";
            
        }

        public async Task<string> GenerateTokenString(UserLogin userLogin,List<string> roles)
        {

            //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value));
            //SigningCredentials signingCred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //IEnumerable<Claim> claims = new List<Claim>()
            //{
            //    new Claim(ClaimTypes.Name,user.UserName),
            //    //new Claim(ClaimTypes.Role , "admin"),
            //    new Claim(ClaimTypes.Country,"Pakistan")
            //};
            //var securityToken = new JwtSecurityToken(
            //    claims: claims,
            //    expires: DateTime.Now.AddMinutes(60),
            //    issuer: _config.GetSection("Jwt:Issuer").Value,
            //    audience: _config.GetSection("Jwt:Audience").Value,
            //    signingCredentials: signingCred
            //    );
            //var tokenString = new JwtSecurityTokenHandler().WriteToken(securityToken);
            //return tokenString;

            
            //httpContext.User.Claims
            SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value));
            SigningCredentials signingCredentials = new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256);

            //var user = await _userManager.GetUserAsync(httpContext.User);
            //var roles = await _userManager.GetRolesAsync(loginUser);
            IEnumerable<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, userLogin.UserName),
                new Claim(ClaimTypes.Role, roles.Single())
            };
            SecurityToken token = new JwtSecurityToken(
                issuer: _config.GetSection("Jwt:Issuer").Value,
                audience: _config.GetSection("Jwt:Audience").Value,
                claims: claims,
                expires: DateTime.Now.AddHours(10),
                signingCredentials:signingCredentials
                );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;
        }
        public async Task<dynamic> GetCurrentRole(HttpContext httpContext)
        {
            var role = httpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role);                      
            return role.Single().Value;
        }
        public async Task<string> UpdateUser(string id, [FromBody] Dictionary<string, string> updates)
        {
            if (string.IsNullOrWhiteSpace(id) || updates == null || updates.Count == 0)
            {
                return " Something is null";
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return "User Not Found";
            }

            foreach (var update in updates)
            {
                Console.WriteLine(typeof(IdentityUser));
                var propertyInfo = typeof(IdentityUser).GetProperty(update.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                //var propertyInfo = typeof(IdentityUser).GetpropertyInfo(update.Key);
                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    var propertyType = Convert.ChangeType(update.Value, propertyInfo.PropertyType);
                    propertyInfo.SetValue(user, propertyType);
                }
                else
                {
                    return null;
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return "User updated successfully";
            }
            return result.ToString();
        }


    }
}
