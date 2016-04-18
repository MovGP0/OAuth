using System;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Owin;

namespace AuthorizationServer
{
    public static class VerifyTokenApplication
    {
        public static void VerifyToken(this IAppBuilder app)
        {
            app.Run(VerifyToken);
        }

        private static async Task VerifyToken(IOwinContext context)
        {
            var query = HttpUtility.ParseQueryString(context.Request.Uri.Query);
            var token = query["access_token"];

            if (string.IsNullOrEmpty(token))
                return;

            // check if token is valid

            // if token (is valid)
            var jwtToken = CreateJwtToken();

            var bytes = Encoding.UTF8.GetBytes(jwtToken);
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }

        private static string CreateJwtToken()
        {
            using (var hmac = new HMACSHA256(Convert.FromBase64String("c2hhcmVkIHNlY3JldA==")))
            {
                const string signatureAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256";
                const string digestAlgorithm = "http://www.w3.org/2001/04/xmlenc#sha256";

                var signingCredentials = new SigningCredentials(new InMemorySymmetricSecurityKey(hmac.Key),
                    signatureAlgorithm, digestAlgorithm);

                var claims = new[]
                {
                    new Claim(ClaimTypes.Email, "foo@bar.com"),
                };

                var token = new JwtSecurityToken(
                    "http://authorizationserver.com/",
                    "http://resourceserver.com/",
                    claims,
                    DateTime.UtcNow,
                    DateTime.UtcNow.AddMinutes(15),
                    signingCredentials);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
        }
    }
}