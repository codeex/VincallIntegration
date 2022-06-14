using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VincallIntegration.Service.Tools
{
    public static class StringExtensions
    {
        public static Uri BuildUrl(this string url, IDictionary<string, StringValues> parameters)
        {
            var uri = new UriBuilder(url);

            var queryString = QueryHelpers.ParseQuery(uri.Query);
            if (parameters != null)
            {
                foreach (var para in parameters)
                {
                    if (queryString.ContainsKey(para.Key))
                    {
                        queryString[para.Key] = StringValues.Concat(queryString[para.Key], para.Value);
                    }
                    else
                    {
                        queryString[para.Key] = para.Value;
                    }
                }
            }
            if (queryString.ContainsKey(string.Empty))
            {
                queryString.Remove(string.Empty);
            }
            uri.Query = QueryString.Create(queryString).ToUriComponent();

            return uri.Uri;
        }

    }
}
