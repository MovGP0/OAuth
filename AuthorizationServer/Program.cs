using System;
using Microsoft.Owin.Hosting;

namespace AuthorizationServer
{
    internal static class Program
    {
        private static void Main()
        {
            using(WebApp.Start<Startup>("http://localhost:5000"))
            {
                Console.WriteLine("Server is active. Press enter to stop.");
                Console.ReadLine();
            }
        }
    }

    // TODO: resource server wants to verify token and get user data with the JWT token 
    // => verify JWT token 
    // => return user data JSON 
}
