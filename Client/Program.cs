using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace Client
{
    internal static class Program
    {
        private const string AuthorizationServerAddress = "http://localhost:5000";
        private const string ResourceServerAddress = "http://localhost:6000";

        private static void Main()
        {
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            // TODO: TryGetResoure
            // TODO: Redirect to AuthServer
            var result = await GetTokenAsync(ResourceServerAddress);
            // TODO: TryGetResoureWithToken
        }

        private static async Task<string> GetTokenAsync(string redirectUrl)
        {
            var redirect = HttpUtility.UrlEncode(redirectUrl);
            var appId = "resourceServerId";
            var clientAppSecret = "clientApplicationSecret";
            
            // POST 
            // https://localhost:5000/oauth/access_token
            //    ?client_id={appId}
            //    &client_secret={clientAppSecret}
            //    &redirect_uri={redirect}
            //    &scope=SPACE SEPARATED LIST OF REQUIRED SCOPES
            
            HttpResponseMessage response;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(AuthorizationServerAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                response = await client.PostAsync($"oauth/?client_id={appId}&redirect_uri={redirect}&client_secret={clientAppSecret}", null);
            }

            return response.Content.ToString();

            // Server responds with HTTP 403 Forbidden
            // asks the user to authenticate 

            // user sends credentials to auth server 

            // server responds with 
            // HTTP 301 Redirect to {ResourceServerAddress}/auth?code=4/P7q7W91a-oMsCeLvIaQm6bTrgtp7
        }
    }
}
