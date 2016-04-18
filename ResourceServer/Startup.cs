using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace ResourceServer
{
    public sealed class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseErrorPage();
            app.AuthenticateUserWithOAuthAndSetCookie();

            app.UseWelcomePage("/");
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = new PathString("/img"),
                FileSystem = new PhysicalFileSystem(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img"))
            });
            app.Run(Foo);
        }

        private static Task<bool> Foo(IOwinContext context)
        {
            return Task.FromResult(true);
        }
    }
}