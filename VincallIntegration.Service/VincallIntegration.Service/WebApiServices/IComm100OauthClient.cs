using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VincallIntegration.Service.Models;
using WebApiClientCore;
using WebApiClientCore.Attributes;

namespace VincallIntegration.Service.WebApiServices
{
    [LoggingFilter]
    public interface IComm100OauthClient : IHttpApi
    {
        [HttpPost("/oauth/token")]
        [HostFilterAttribute]
        Task<TokenResult> QueryAccessTokenAsync([PathQuery]string siteId, [FormContent]string code, [FormContent]string client_id, [FormContent]string client_secret, [FormContent]string redirect_uri, [FormContent]string grant_type = "authorization_code");

        [HttpPost("/connect/token")]
        [HostFilterAttribute]
        Task<TokenResult> VincallQueryAccessTokenAsync([FormContent]string code, [FormContent]string client_id, [FormContent]string client_secret, [FormContent]string redirect_uri, [FormContent]string grant_type = "authorization_code");

        /// <summary>
        /// Bearer type
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("/oauth/userinfo")]
        [HostFilterAttribute]
        ITask<Comm100Info> GetProfileInfoAsync([Header("Authorization")]string token,[PathQuery]string siteId);

    }   
}
