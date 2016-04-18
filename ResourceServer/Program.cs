using System;
using Microsoft.Owin.Hosting;

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
    }
}
