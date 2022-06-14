using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VincallIntegration.Application.Oauth;
using VincallIntegration.Infrastructure;

namespace VincallIntegration.Application
{
    public class GlobalSettingService
    {
        private readonly IMemoryCache _cache;
        private readonly ICrudServices _service;

        public GlobalSettingService(IMemoryCache cache,ICrudServices service)
        {
            _cache = cache;
            _service = service;
        }

        public  Task<Comm100Oauth> GetAsync(string type)
        {
            return  _cache.GetOrCreateAsync(type, async cache =>
            {
                var settings = await _service.ReadMany<GlobalSetting>(x => x.Type == type).ToListAsync();
                cache.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return new Comm100Oauth
                {
                    client_id = settings.FirstOrDefault(x => string.Equals(x.Key, "client_id", StringComparison.InvariantCultureIgnoreCase))?.Value,
                    client_secret = settings.FirstOrDefault(x => string.Equals(x.Key, "client_secret", StringComparison.InvariantCultureIgnoreCase))?.Value,
                    scope = settings.FirstOrDefault(x => string.Equals(x.Key, "scope", StringComparison.InvariantCultureIgnoreCase))?.Value,
                    grant_type = settings.FirstOrDefault(x => string.Equals(x.Key, "grant_type", StringComparison.InvariantCultureIgnoreCase))?.Value ?? "authorization_code",
                    redirect_uri = settings.FirstOrDefault(x => string.Equals(x.Key, "redirect_uri", StringComparison.InvariantCultureIgnoreCase))?.Value,
                    redirect_logon = settings.FirstOrDefault(x => string.Equals(x.Key, "redirect_logon", StringComparison.InvariantCultureIgnoreCase))?.Value,
                };
            });
            
        }

    }
}
