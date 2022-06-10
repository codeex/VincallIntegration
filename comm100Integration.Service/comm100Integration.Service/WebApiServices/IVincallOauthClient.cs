using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Comm100Integration.Service.Models;
using WebApiClientCore;
using WebApiClientCore.Attributes;

namespace Comm100Integration.Service.WebApiServices
{
    [LoggingFilter]
    public interface IVincallOAuthService : IHttpApi
    {
        [HostFilterAttribute]
        [HttpPost("/connect/token")]
        ITask<TokenResult> GetVincallTokenAsync([FormContent]string client_id = "connect", [FormContent]string client_secret = "vincall.net.2022",[FormContent] string scope= "connectapi", [FormContent]string grant_type = "client_credentials");
    }
}
