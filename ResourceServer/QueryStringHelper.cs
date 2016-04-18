using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace ResourceServer
{
    public static class QueryStringHelper
    {
        public static string ToQueryString(this NameValueCollection source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            
            var sb = new StringBuilder();

            foreach (var key in source.AllKeys)
            {
                var encodedKey = HttpUtility.HtmlEncode(key);
                var encodedValue = HttpUtility.HtmlEncode(source[key]);
                sb.Append($"{encodedKey}={encodedValue}&");
            }
            
            return sb.ToString();
        }
    }
}