using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Comm100Integration.Application
{
    public static class CookieHelper
    {
        public static string Name => ".AspNet.SharedCookie";        

        public static string GetDomainName(string hostUri)
        {
            if (string.IsNullOrEmpty(hostUri))
            {
                throw new ArgumentNullException("hostUri");
            }
            return hostUri.Substring(hostUri.IndexOf('.')+1);
        }
    }
    
}
