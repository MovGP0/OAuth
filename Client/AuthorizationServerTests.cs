using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using AuthorizationServer;
using FluentAssert;
using Microsoft.Owin.Testing;
using Xunit;

namespace Client
{
    public sealed class AuthorizationServerTests : IDisposable
    {
        private const string AuthorizationServerAddress = "http://localhost:5000";
        private const string ResourceServerAddress = "http://localhost:5001";

        // Server responds with HTTP 403 Forbidden
        // asks the user to authenticate 

        private TestServer TestServer { get; }

        public AuthorizationServerTests()
        {
            TestServer = TestServer.Create<Startup>();
        }

        ~AuthorizationServerTests()
        {
            Dispose();
        }

        [Fact]
        private async Task GetTokenAsync_MustRedirect()
        {
            var response = await GetTokenAsyncInternal();

            response.StatusCode.ShouldBeEqualTo(HttpStatusCode.RedirectMethod);
        }

        [Fact]
        private async Task GetTokenAsync_TargetLocationMustBeCorrectlySet()
        {
            var response = await GetTokenAsyncInternal();

            response.Headers.Location.Authority.ShouldBeEqualTo(new Uri(ResourceServerAddress).Authority);
        }

        [Fact]
        private async Task GetTokenAsync_TokenMustBeAvailable()
        {
            var response = await GetTokenAsyncInternal();

            var nameValues = HttpUtility.ParseQueryString(response.Headers.Location.Query);
            nameValues["code"].ShouldNotBeEmpty();
        }

        [Fact]
        private async Task GetTokenAsync_TokenMustHaveRightLength()
        {
            var response = await GetTokenAsyncInternal();

            var nameValues = HttpUtility.ParseQueryString(response.Headers.Location.Query);
            (nameValues["code"] ?? string.Empty).Length.ShouldBeEqualTo(24);
        }

        private async Task<HttpResponseMessage> GetTokenAsyncInternal()
        {
            var redirect = HttpUtility.UrlEncode(ResourceServerAddress);
            const string appId = "resourceServerId";
            const string clientAppSecret = "clientApplicationSecret";

            var client = TestServer.HttpClient;
            client.BaseAddress = new Uri(AuthorizationServerAddress);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await client.PostAsync($"oauth/access_token?client_id={appId}&redirect_uri={redirect}&client_secret={clientAppSecret}", null);
        }

        private bool _isDisposed;
        public void Dispose()
        {
            Dispose(!_isDisposed);
            _isDisposed = true;
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;
            TestServer.Dispose();
        }
    }
}
