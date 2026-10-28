using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DroneRL_backend.Dtos;
using DroneRL_backend.Models;
using DroneRL_backend.Services;

namespace DroneRL_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ApiControllerBase
    {
        private readonly AuthService service;

        public AuthController(AuthService service)
        {
            this.service = service;
        }

        // Вход по логину и паролю → JWT-токен
        [HttpPost("login")]
        public async Task<ActionResult<TokenDto>> Login([FromBody] LoginDto dto)
        {
            var token = await service.LoginAsync(dto);
            if (token == null)
                return Unauthorized(new { message = "Wrong login or password" });
            return token;
        }

        // Кто я: данные берутся из токена
        [HttpGet("me")]
        [Authorize]
        public UserDto Me()
        {
            return new UserDto
            {
                Login = User.Identity.Name,
                Role = User.IsInRole(Roles.Admin) ? Roles.Admin : Roles.User
            };
        }

        [HttpPost("register")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto dto)
        {
            return await ExecuteCreatedAsync(() => service.RegisterAsync(dto), nameof(GetUsers), u => null);
        }

        [HttpGet("users")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<List<UserDto>>> GetUsers()
        {
            return await ExecuteAsync(() => service.GetUsersAsync());
        }
    }
}
