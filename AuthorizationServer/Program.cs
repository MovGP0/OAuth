namespace AuthorizationServer
{
    internal static class Program
    {
        private static void Main()
        {
            // TODO: user gets redirected and requests authentication
            // => return HTTP 403 Forbidden
            //    and ask the user to authenticate 

            // TODO: user sends username and password 
            // give the user a redirect to the resource URI with the token 

            // TODO: resource server wants to verify token and get user data with the JWT token 
            // => verify JWT token 
            // => return user data JSON 
        }
    }
}
