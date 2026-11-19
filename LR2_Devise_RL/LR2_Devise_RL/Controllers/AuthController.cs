using LR2_Devise_RL.Data;
using LR2_Devise_RL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LR2_Devise_RL.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        public class LoginRequest
        {
            public string Login { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Login)
                || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Введите логин и пароль.");
            }

            var identity = GetIdentity(request.Login, request.Password);

            if (identity == null)
            {
                return Unauthorized("Неверный логин или пароль.");
            }

            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: identity.Claims,
                expires: now.AddMinutes(AuthOptions.LIFETIME),
                signingCredentials: new SigningCredentials(
                    AuthOptions.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256)
            );

            var encodedJwt = new JwtSecurityTokenHandler()
                .WriteToken(jwt);

            return Ok(new
            {
                access_token = encodedJwt,
                username = identity.Name
            });
        }

        private ClaimsIdentity? GetIdentity(string login, string password)
        {
            var user = _context.Users
                .FirstOrDefault(user => user.Login == login);

            if (user == null)
            {
                return null;
            }

            var hasher = new PasswordHasher<User>();

            var verification = hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password);

            if (verification == PasswordVerificationResult.Failed)
            {
                return null;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role)
            };

            return new ClaimsIdentity(
                claims,
                "Token",
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
        }
    }
}
