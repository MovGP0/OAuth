using System;

namespace AuthorizationServer
{
    public sealed class AccesTokenRequest
    {
        public string ClientId { get; }
        public string ClientSecret { get; }
        public Uri RedirectUri { get; }

        public AccesTokenRequest(string clientId, string clientSecret, Uri redirectUri)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            RedirectUri = redirectUri;
        }
    }
}