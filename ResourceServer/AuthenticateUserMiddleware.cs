using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.ServiceModel.Security.Tokens;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace ResourceServer
{
    internal sealed class AuthenticateUserMiddleware
    {
        private Func<IDictionary<string, object>, Task> Next { get; }

        public AuthenticateUserMiddleware(Func<IDictionary<string, object>, Task> next)
        {
            Next = next;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            IOwinContext context = new OwinContext(env);

            var token = GetTokenFromRequestOrNull(context.Request);
            var hasToken = token != null;

            var cookie = context.Request.Cookies["com.ressourceserver.authcookie"];
            var hasCookie = cookie != null;

            if(!hasCookie && !hasToken)
            {
                RedirectToAuthServer(context);
                return;
            }

            if(hasToken)
            {
                var principal = await GetClaimsPrincipalForTokenOrNull(token);
                SetAuthCookie(principal, context);
                Refresh(context);
                return;
            }

            var ticket = FormsAuthentication.Decrypt(cookie);
            if(ticket == null || ticket.Expired)
            {
                RedirectToAuthServer(context);
                return;
            }

            // user is authenticated
            await Next.Invoke(env);
        }

        private static void SetAuthCookie(ClaimsPrincipal principal, IOwinContext context)
        {
            var email = principal.Claims.Where(c => c.Type == ClaimTypes.Email).Select(c => c.Value).FirstOrDefault();
            var newCookie = CreateAuthentificationCookie(email);
            context.Response.Cookies.Append("com.ressourceserver.authcookie", newCookie, new CookieOptions
            {
                Expires = DateTime.UtcNow.AddHours(24),
                Path = "/",
                Domain = "localhost",
                HttpOnly = false,
                Secure = false
            });
        }

        private static void Refresh(IOwinContext context)
        {
            var query = HttpUtility.ParseQueryString(context.Request.Uri.Query);
            
            var redirectUri = new UriBuilder(context.Request.Uri)
            {
                Query = query.ToQueryString()
            }.Uri;
            
            context.Response.OnSendingHeaders(state =>
            {
                var response = (OwinResponse) state;
                response.StatusCode = (int) HttpStatusCode.RedirectMethod;
                response.Headers[HttpResponseHeader.Location.ToString()] = redirectUri.ToString();
            }, context.Response);
        }

        private static string GetTokenFromRequestOrNull(IOwinRequest request)
        {
            var query = request.Uri.Query;

            if(string.IsNullOrEmpty(query))
                return null;

            var parsedQuery = HttpUtility.ParseQueryString(query);
            var code = parsedQuery["code"];

            if(string.IsNullOrEmpty(code))
                return null;

            return code;
        }

        private const string AuthorizationServerAddress = "http://localhost:5000";
        
        private static async Task<ClaimsPrincipal> GetClaimsPrincipalForTokenOrNull(string token)
        {
            var requestUriWithoutQuery = new UriBuilder(AuthorizationServerAddress)
            {
                Path = "/oauth/verify_token"
            }.Uri;

            var query = HttpUtility.ParseQueryString(requestUriWithoutQuery.ToString());
            query["access_token"] = token;

            var requestUri = new UriBuilder(AuthorizationServerAddress)
            {
                Path = "/oauth/verify_token",
                Query = query.ToQueryString()
            }.Uri;

            using(var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync(requestUri);
                var tokenContent = response.Content as StringContent;
                if(tokenContent == null)
                    return null;

                var tokenString = tokenContent.ToString();

                // check JWT token 
                using(var hmac = new HMACSHA256(Convert.FromBase64String("c2hhcmVkIHNlY3JldA==")))
                {
                    const string signatureAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256";
                    const string digestAlgorithm = "http://www.w3.org/2001/04/xmlenc#sha256";

                    var signingCredentials = new SigningCredentials(new InMemorySymmetricSecurityKey(hmac.Key), signatureAlgorithm, digestAlgorithm);

                    var validationParams = new TokenValidationParameters
                    {
                        ValidIssuer = "http://authorizationserver.com/",
                        ValidAudience = "http://resourceserver.com/",
                        ValidateLifetime = true,
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        IssuerSigningToken = new BinarySecretSecurityToken(hmac.Key),
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingCredentials.SigningKey,
                        ValidateActor = false
                    };

                    SecurityToken validatedToken;
                    return new JwtSecurityTokenHandler().ValidateToken(tokenString, validationParams, out validatedToken);
                }
            }
        }

        private static string CreateAuthentificationCookie(string emailAddress)
        {
            var userJson = JsonConvert.SerializeObject(new
            {
                Email = emailAddress
            });

            var authTicket = new FormsAuthenticationTicket(1, emailAddress, DateTime.UtcNow, DateTime.UtcNow.AddHours(24), false, userJson);
            var ticket = FormsAuthentication.Encrypt(authTicket);
            return ticket;
        }

        private static void RedirectToAuthServer(IOwinContext context)
        {
            var targetUri = new UriBuilder("http", "localhost", 5000, "/oauth/access_token");
            var query = HttpUtility.ParseQueryString(targetUri.Query);
            query["client_id"] = "myClient";
            query["client_secret"] = "TopSecret";
            query["redirect_uri"] = context.Request.Uri.ToString();
            targetUri.Query = query.ToQueryString();

            context.Response.OnSendingHeaders(state =>
            {
                var response = (OwinResponse)state;
                response.StatusCode = (int)HttpStatusCode.RedirectMethod;
                response.Headers[HttpResponseHeader.Location.ToString()] = targetUri.Uri.ToString();
            }, context.Response);
        }
    }
}