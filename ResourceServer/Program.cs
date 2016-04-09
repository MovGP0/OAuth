using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace ResourceServer
{
    internal static class Program
    {

        private static void Main()
        {
            using(WebApp.Start<Startup>("http://localhost:5001"))
            {
                Console.WriteLine("Server is active on port 5001. Press enter to stop.");
                Console.ReadLine();
            }
        }

        // TODO: User gets resource without token 
        // => redirect to oauth server 

        // TODO: User gets resource with authorization code
        // => verify authorization code with auth server via JWT token
        //    and (optionally) get additional profile info
        // => when valid give user authentication cookie
        // => redirect to resource 

        // TODO: User gets resource with resource server token
        // => give user the resource 
    }

    public sealed class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseErrorPage();
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
