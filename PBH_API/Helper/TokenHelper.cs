using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PBH_API.Helper
{
    public class TokenHelper
    {
            public string GenerateToken()
            {
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var sb = new StringBuilder();

            for (int i = 0; i < 16; i++)
            {
                int index = random.Next(characters.Length);
                sb.Append(characters[index]);
            }

            return sb.ToString();
        }
    }
}
