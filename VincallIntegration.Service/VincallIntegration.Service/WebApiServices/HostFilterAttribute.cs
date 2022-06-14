using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiClientCore;
using WebApiClientCore.Attributes;

namespace VincallIntegration.Service.WebApiServices
{
    public class HostFilterAttribute : ApiFilterAttribute
    {
       
        public override Task OnRequestAsync(ApiRequestContext context)
        {
            var options = context.HttpContext.HttpApiOptions;
            options.Properties.TryGetValue("serviceName", out object serviceNameObject);
            string serviceName = serviceNameObject as string;
            var sp = context.HttpContext.ServiceProvider;
            var hostProvider = sp.GetRequiredService<HostProvider>();
            HttpApiRequestMessage requestMessage = context.HttpContext.RequestMessage;
            requestMessage.RequestUri = requestMessage.MakeRequestUri(new Uri(hostProvider.Host));

            return Task.CompletedTask;

        }

        public override Task OnResponseAsync(ApiResponseContext context)
        {
            return Task.CompletedTask;
        }
    }

    public class HostProvider
    {
        public string Host { get; set; }
    }
}
