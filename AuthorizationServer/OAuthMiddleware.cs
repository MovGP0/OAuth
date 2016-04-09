using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;

namespace AuthorizationServer
{
    public sealed class OAuthMiddleware
    {
        private Func<IDictionary<string, object>, Task> Next { get; }

        public OAuthMiddleware(Func<IDictionary<string, object>, Task> next)
        {
            Next = next;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            IOwinContext context = new OwinContext(env);

            //var message = "Hello World!";
            //var bytes = Encoding.UTF8.GetBytes(message);
            //context.Response.Body.Write(bytes, 0, bytes.Length);

            var queryString = context.Request.QueryString.Value ?? string.Empty;

            AccesTokenRequest request;
            var success = TryParseAccessTokenRequest(queryString, out request);

            await Next.Invoke(env);
            
            if(!success)
            {
                RespondForbidden(context);
                return;
            }

            RedirectWithToken(context, request.RedirectUri);
        }

        private static void RedirectWithToken(IOwinContext context, Uri targetUri)
        {
            var token = CreateToken();
            var targetUriWithToken = AppendTokenToUri(targetUri, token);

            context.Response.OnSendingHeaders(state =>
            {
                var response = (OwinResponse) state;
                response.StatusCode = (int) HttpStatusCode.RedirectMethod;
                response.Headers[HttpResponseHeader.Location.ToString()] = targetUriWithToken.ToString();
            }, context.Response);
        }

        private static Uri AppendTokenToUri(Uri targetUri, string token)
        {
            var uriBuilder = new UriBuilder(targetUri);
            var query = HttpUtility.ParseQueryString(targetUri.Query);
            query["code"] = token;
            uriBuilder.Query = query.ToString();
            return uriBuilder.Uri;
        }

        private static string CreateToken()
        {
            var tokenBytes = new byte[16];
            using(var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(tokenBytes);
            }
            var token = Convert.ToBase64String(tokenBytes);
            return token;
        }

        private static void RespondForbidden(IOwinContext context)
        {
            context.Response.OnSendingHeaders(state =>
            {
                var response = (OwinResponse) state;
                response.StatusCode = (int) HttpStatusCode.Forbidden;
            }, context.Response);
        }

        private static bool TryParseAccessTokenRequest(string queryString, out AccesTokenRequest request)
        {
            var parameterDictionary = HttpUtility.ParseQueryString(queryString);

            var clientId = parameterDictionary["client_id"] ?? string.Empty;
            var clientSecret = parameterDictionary["client_secret"] ?? string.Empty;
            var redirectUriString = parameterDictionary["redirect_uri"] ?? string.Empty;

            if(clientId == string.Empty || clientSecret == string.Empty || redirectUriString == string.Empty)
            {
                request = null;
                return false;
            }

            Uri redirectUri;
            var isValidUri = Uri.TryCreate(HttpUtility.UrlDecode(redirectUriString), UriKind.Absolute, out redirectUri);

            if(!isValidUri)
            {
                request = null;
                return false;
            }

            request = new AccesTokenRequest(clientId, clientSecret, redirectUri);
            return true;
        }
    }
}