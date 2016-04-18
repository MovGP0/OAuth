using Owin;

namespace ResourceServer
{
    public static class AuthenticateUserExtensions
    {
        public static void AuthenticateUserWithOAuthAndSetCookie(this IAppBuilder app)
        {
            app.Use<AuthenticateUserMiddleware>();
            app.Use(PipelineStage.Authenticate);
        }
    }
}