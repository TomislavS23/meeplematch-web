using System.IdentityModel.Tokens.Jwt;

namespace meeplematch_web.Utils
{
    public class JwtUtils
    {
        public static JwtSecurityToken ConvertJwtStringToJwtSecurityToken(string? jwtString)
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(jwtString);
        }

        public static List<KeyValuePair<string, object>> DecodeJwt(JwtSecurityToken token)
        {
            var payload = token.Payload;
            foreach (var claim in payload)
            {
                Console.WriteLine($"{claim.Key}: {claim.Value}");
            }
            return payload.ToList();
        }
    }
}
