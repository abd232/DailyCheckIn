using DailyCheckIn.Interfaces;
using DailyCheckIn.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DailyCheckIn.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration config;
        private readonly UserManager<AppUser> userManager;

        public TokenService(IConfiguration config, UserManager<AppUser> userManager)
        {
            this.config = config;
            this.userManager = userManager;
        }
        public async Task<string> CreateToken(AppUser user)
        {
            var tokenKey = config["TokenKey"] ?? throw new Exception("Can't access tokenKey from appsetting");
            if (tokenKey.Length < 64)
                throw new Exception("Your tokenKey needs to be longer");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserName ?? ""),
            };

            var roles = await userManager.GetRolesAsync(user); 

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDesctiptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMonths(1)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDesctiptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
