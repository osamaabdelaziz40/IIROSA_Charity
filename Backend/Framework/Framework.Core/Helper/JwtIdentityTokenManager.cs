using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.Helper
{
    public class JwtIdentityTokenManager : IIdentityTokenManager
    {
        private readonly JwtIdentitySettingDto _jwtSettings;

        // Direct injection - no IOptions wrapper needed
        public JwtIdentityTokenManager(JwtIdentitySettingDto jwtSettings)
        {
            _jwtSettings = jwtSettings;

            // Debug logging to verify loading
            if (_jwtSettings == null)
            {
                Console.WriteLine("❌ JWT Settings are NULL - Check appsettings.json and Program.cs configuration");
                throw new InvalidOperationException("JWT Settings are not configured. Check appsettings.json and ensure Program.cs registers JwtIdentitySettingDto.");
            }

            if (string.IsNullOrEmpty(_jwtSettings.Key))
            {
                Console.WriteLine("❌ JWT Key is NULL or Empty - Add 'JwtIdentitySettingDto:Key' to appsettings.json");
                throw new InvalidOperationException("JWT Key is missing. Add 'JwtIdentitySettingDto:Key' to appsettings.json");
            }

            Console.WriteLine($"✅ JWT IdentityTokenManager Initialized:");
            Console.WriteLine($"   Key Length: {_jwtSettings.Key.Length}");
            Console.WriteLine($"   Issuer: {_jwtSettings.Issuer}");
            Console.WriteLine($"   Audience: {_jwtSettings.Audience}");
        }

        //private ClaimsPrincipal? GetPrincipal(string token)
        //{
        //    try
        //    {
        //        token = token.Replace("Bearer ", "");
        //        var tokenHandler = new JwtSecurityTokenHandler();
        //        var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

        //        if (jwtToken == null)
        //            return null;

        //        var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key ?? string.Empty));

        //        var validationParameters = new TokenValidationParameters()
        //        {
        //            RequireExpirationTime = true,
        //            ValidateIssuer = true,
        //            ValidIssuer = _jwtSettings.Issuer,
        //            ValidateAudience = true,
        //            ValidAudience = _jwtSettings.Audience,
        //            IssuerSigningKey = symmetricKey
        //        };

        //        SecurityToken securityToken;
        //        var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);
        //        var name = jwtToken.Claims?.FirstOrDefault(x => x.Type.Equals("name", StringComparison.OrdinalIgnoreCase))?.Value;

        //        return name;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
    
        public string? GetCurrentUserName(string token)
        {
            if(token == null || token == "null" || token == "Bearer null") 
            {
                return null;
            }
            token = token.Replace("Bearer ", "");
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                return null;

            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key ?? string.Empty));

            var validationParameters = new TokenValidationParameters()
            {
                RequireExpirationTime = true,
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = symmetricKey
            };

            //var name = jwtToken.Claims?.Where(c => c.Type == ClaimTypes.Name)?.Select(c => c.Value).SingleOrDefault();
            var name = jwtToken.Claims?.FirstOrDefault(x => x.Type.Equals("name", StringComparison.OrdinalIgnoreCase))?.Value;

            return name;
        
        }
    }
}
