using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace LR2_Devise_RL.Models
{
    public class AuthOptions
    {
        public const string ISSUER = "DroneApi"; // издатель токена
        public const string AUDIENCE = "DroneApiClient"; // потребитель токена
        const string KEY = "DroneApi_LocalDemo_SecretKey_1234567890";   // ключ для учебного проекта
        public const int LIFETIME = 60; // время жизни токена в минутах
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
