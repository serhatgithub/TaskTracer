
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration; // Bu using eklenecek
using Microsoft.IdentityModel.Tokens;
using TaskTracer.AuthService.Models; // Model namespace'ini güncelledik

namespace TaskTracer.AuthService.Services // Namespace'i güncelledik
{
    public class TokenService
    {
        private readonly IConfiguration _config;
        public TokenService(IConfiguration config) => _config = config;

        public string CreateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username)
                // İleride UserService veya TaskService bu token'ı doğrularken
                // ek claim'ler (örn: roller) gerekebilir, şimdilik bunlar yeterli.
            };

            // appsettings.json dosyasından JWT ayarlarını okuyacağız
            // Bu ayarların TaskTracer.AuthService/appsettings.json dosyasına eklenmesi gerekecek.
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7), // Token geçerlilik süresi
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}