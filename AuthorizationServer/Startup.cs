using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

namespace AuthorizationServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseErrorPage();
            app.Map("/oauth", oauthApp =>
            {
                oauthApp.Map("/access_token", accessTokenApp =>
                {
                    accessTokenApp.Use<OAuthMiddleware>();
                    accessTokenApp.Run(Foo);
                });

                oauthApp.Map("/verify_token", verifyTokenApp =>
                {
                    verifyTokenApp.VerifyToken();
                });
            });
        }

        private static Task<bool> Foo(IOwinContext context)
        {
            return Task.FromResult(true);
        }
    }
}