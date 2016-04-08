namespace ResourceServer
{
    internal static class Program
    {
        private static void Main()
        {
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
    }
}
