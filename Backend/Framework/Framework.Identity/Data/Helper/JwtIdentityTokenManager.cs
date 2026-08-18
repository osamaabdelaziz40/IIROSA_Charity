//using Microsoft.Extensions.Options;
//using Microsoft.IdentityModel.Tokens;
//using System;
//using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;

//namespace Framework.Identity.Data.Helper
//{
//    public class JwtIdentityTokenManager : IIdentityTokenManager
//    {
//        private readonly JwtIdentitySettingDto _jwtSettings;

//        public JwtIdentityTokenManager(IOptions<JwtIdentitySettingDto> jwtSettings)
//        {
//            _jwtSettings = jwtSettings.Value;
//        }

//        //private ClaimsPrincipal? GetPrincipal(string token)
//        //{
//        //    try
//        //    {
//        //        token = token.Replace("Bearer ", "");
//        //        var tokenHandler = new JwtSecurityTokenHandler();
//        //        var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

//        //        if (jwtToken == null)
//        //            return null;

//        //        var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key ?? string.Empty));

//        //        var validationParameters = new TokenValidationParameters()
//        //        {
//        //            RequireExpirationTime = true,
//        //            ValidateIssuer = true,
//        //            ValidIssuer = _jwtSettings.Issuer,
//        //            ValidateAudience = true,
//        //            ValidAudience = _jwtSettings.Audience,
//        //            IssuerSigningKey = symmetricKey
//        //        };

//        //        SecurityToken securityToken;
//        //        var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);
//        //        var name = jwtToken.Claims?.FirstOrDefault(x => x.Type.Equals("name", StringComparison.OrdinalIgnoreCase))?.Value;

//        //        return name;
//        //    }
//        //    catch
//        //    {
//        //        return null;
//        //    }
//        //}
    
//        public string? GetCurrentUserName(string token)
//        {
//            token = token.Replace("Bearer ", "");
//            var tokenHandler = new JwtSecurityTokenHandler();
//            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

//            if (jwtToken == null)
//                return null;

//            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key ?? string.Empty));

//            var validationParameters = new TokenValidationParameters()
//            {
//                RequireExpirationTime = true,
//                ValidateIssuer = true,
//                ValidIssuer = _jwtSettings.Issuer,
//                ValidateAudience = true,
//                ValidAudience = _jwtSettings.Audience,
//                IssuerSigningKey = symmetricKey
//            };

//            //var name = jwtToken.Claims?.Where(c => c.Type == ClaimTypes.Name)?.Select(c => c.Value).SingleOrDefault();
//            var name = jwtToken.Claims?.FirstOrDefault(x => x.Type.Equals("name", StringComparison.OrdinalIgnoreCase))?.Value;

//            return name;
        
//        }
//    }
//}
