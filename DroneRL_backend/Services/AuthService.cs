using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using DroneRL_backend.Dtos;
using DroneRL_backend.Exceptions;
using DroneRL_backend.Models;

namespace DroneRL_backend.Services
{
    // Параметры токена из раздела "Jwt" в appsettings.json
    public class JwtSettings
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
        public int LifetimeMinutes { get; set; }

        public SymmetricSecurityKey GetSigningKey() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
    }

    public class AuthService
    {
        private readonly AppDbContext db;
        private readonly JwtSettings jwt;

        public AuthService(AppDbContext db, JwtSettings jwt)
        {
            this.db = db;
            this.jwt = jwt;
        }

        // Проверяет логин и пароль; при успехе выдаёт JWT-токен, иначе возвращает null
        public TokenDto Login(LoginDto dto)
        {
            var user = db.Users.FirstOrDefault(u => u.Login == dto.Login);
            if (user == null || !user.CheckPassword(dto.Password))
                return null;
            return GenerateToken(user);
        }

        public UserDto Register(RegisterDto dto)
        {
            var login = dto.Login.Trim();
            if (db.Users.Any(u => u.Login == login))
                throw new BusinessRuleException($"User '{login}' already exists");

            var user = new User { Login = login, Role = dto.Role ?? Roles.User };
            user.SetPassword(dto.Password);
            db.Users.Add(user);
            db.SaveChanges();
            return UserDto.From(user);
        }

        public List<UserDto> GetUsers()
        {
            return db.Users.OrderBy(u => u.Id).ToList().Select(UserDto.From).ToList();
        }

        private TokenDto GenerateToken(User user)
        {
            var expires = DateTime.UtcNow.AddMinutes(jwt.LifetimeMinutes);
            var claims = new List<Claim>
            {
                new Claim("name", user.Login),
                new Claim("role", user.Role)
            };
            var token = new JwtSecurityToken(
                issuer: jwt.Issuer,
                audience: jwt.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: new SigningCredentials(jwt.GetSigningKey(), SecurityAlgorithms.HmacSha256));

            return new TokenDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Login = user.Login,
                Role = user.Role,
                ExpiresAt = expires
            };
        }
    }
}
