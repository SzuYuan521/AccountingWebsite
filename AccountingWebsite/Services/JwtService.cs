using AccountingWebsite.Models;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AccountingWebsite.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 生成 JWT Token 的方法
        /// </summary>
        /// <param name="user"></param>
        /// <returns>token(string)</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public string GenerateJWTToken(User user)
        {
            // 設置 Claims，這些是 Token 中包含的用戶信息
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // 用戶唯一標識符
                new Claim(ClaimTypes.Name, user.UserName), // 用戶名
                new Claim(ClaimTypes.Email, user.Email), // 用戶電子郵件
            };

            Debug.WriteLine($"Jwt:Key [{_configuration["Jwt:Key"]}]");

            // 從配置中讀取密鑰並創建 SymmetricSecurityKey
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            // 創建簽名憑證
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 確保 expireMinutes 是有效的正數
            var expireMinutes = Convert.ToDouble(_configuration["Jwt:JwtTokenExpireMinutes"]);
            if (expireMinutes <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expireMinutes), "expireMinutes 必須是正數");
            }

            // 設置 Token 的過期時間
            var expires = DateTime.UtcNow.AddMinutes(expireMinutes);

            // 創建 JwtSecurityToken 實例, 設定Claims、過期時間和簽名憑證
            var token = new JwtSecurityToken(
                claims: claims, // Token 中包含的 Claims
                expires: expires, // Token 過期時間
                signingCredentials: creds // Token 簽名憑證
            );

            // 將 JwtSecurityToken 實例轉換為字符串
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            Debug.WriteLine($"JWT Token for user {user.Id} is {tokenString}");

            return tokenString;
        }
    }
}
