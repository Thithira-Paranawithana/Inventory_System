using Inventory_System.Data;
using Inventory_System.DTOs;
using Inventory_System.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;


namespace Inventory_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly InventoryDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(InventoryDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // validate input
                if(!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // find user in db by username
                var user = await _context.Users
                    .Include(u => u.Store)  // include store info
                    .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

                if(user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                // generate JWT token
                var jwtToken = GenerateJwtToken(user);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,  // JS cannot access cooki, xss protect
                    Secure = Request.IsHttps, // only https
                    SameSite = SameSiteMode.Strict, // CSRF protect
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                };

                Response.Cookies.Append("AccessToken", jwtToken, cookieOptions);

                // auth cookie
                await CreateAuthenticationCookie(user);

                return Ok(new LoginResponseDto
                {
                    Token = jwtToken,
                    Username = user.Username,
                    Role = user.Role,
                    StoreId = user.StoreId ?? 0  // 0 for clients 
                });

            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "Error occured in login", error = ex.Message });
            }
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // clear jwt token cookie
                Response.Cookies.Delete("AccessToken");

                // signout from cookie authentication
                await HttpContext.SignOutAsync("Cookies");

                return Ok(new { message = "Logged out" });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "Error occured in logout", error = ex.Message });
            }
        }


        // generate token
        private string GenerateJwtToken(User user)
        {
            // user info (claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Iat, // Issued at time
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            if (user.StoreId.HasValue)
            {
                claims.Add(new Claim("StoreId", user.StoreId.Value.ToString()));
            }

            // get secret key from configuration
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));

            // Create signing 
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // create token
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"], // issuer
                audience: _configuration["JwtSettings:Audience"], // audience
                claims: claims, // user info
                expires: DateTime.UtcNow.AddHours(1), 
                signingCredentials: credentials 
            );

            // Convert token to string format
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //  authentication cookie for web login
        private async Task CreateAuthenticationCookie(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
            };

            if (user.StoreId.HasValue)
            {
                claims.Add(new Claim("StoreId", user.StoreId.Value.ToString()));
            }

            // Create claims identity
            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

            // create auth properties
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // cookie stays when browser restart
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1) 
            };

            // sign in user with cookie authentication
            await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity),authProperties);

        }



    }
}
