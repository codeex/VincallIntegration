using VincallIntegration.Application;
using VincallIntegration.Infrastructure;
using VincallIntegration.Service.Models;
using VincallIntegration.Service.WebApiServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VincallIntegration.Service.Tools;
using Microsoft.Extensions.Configuration;

namespace VincallIntegration.Service.Controllers
{

    [ApiController]
    public class LoginController:ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly ICrudServices _services;
        private readonly IComm100OauthClient _comm100OauthClient;
        private readonly HostProvider _hostProvider;
        private readonly GlobalSettingService _settingService;
        private readonly IVincallOAuthService _vincallClient;
        private readonly IConfiguration _Configuration;


        public LoginController(ILogger<LoginController> logger, IComm100OauthClient comm100OauthClient, GlobalSettingService settingService, HostProvider hostProvider, ICrudServices services, IVincallOAuthService vincallClient, IConfiguration Configuration)
        {
            _logger = logger;
            _comm100OauthClient = comm100OauthClient;
            _settingService = settingService;
            _hostProvider = hostProvider;
            _services = services;
            _vincallClient = vincallClient;
            _Configuration = Configuration;
        }

        [HttpGet("login")]
        public async Task<IActionResult> LoginAsync([FromQuery]LoginModel model)
        {
            if (string.IsNullOrEmpty(model?.Domain))
            {
                return new BadRequestResult();
            }
            var oauthInfo = await _settingService.GetAsync("connect");
            if (string.IsNullOrEmpty(oauthInfo?.client_id))
            {
                return new BadRequestResult();
            }

            var redirectUri = oauthInfo.redirect_uri.BuildUrl(
                new Dictionary<string, StringValues> {
                            {"siteid",model.SiteId },
                            {"domain", model.Domain },
                            {"agentId", model.AgentId },
                            {"returnUri", model.returnUri },
                        }
                );

            var comm100OauthUri = $"{model.Domain}/oauth/authorize".BuildUrl(
                new Dictionary<string, StringValues> {
                            {"client_id",oauthInfo.client_id },
                            {"response_type", "code" },
                            {"siteid", model.SiteId },
                            {"scope", "openid offline_access"},
                            {"redirect_uri", redirectUri.ToString() },
                        }
                );
            _logger.LogInformation($"redirect oauth: {comm100OauthUri.ToString()}");
            return Redirect(comm100OauthUri.ToString());
        }

        [HttpGet("login/comm100callback")]
        public async Task<IActionResult> Comm100callbackAsync([FromQuery]Comm100CallbackModel model)
        {
            if (string.IsNullOrEmpty(model?.returnUri))
            {
                _logger.LogError("returnUri is null");
                return new BadRequestResult();
            }
            var errMsg = string.Empty;
            var oauthInfo = await _settingService.GetAsync("connect");
            if (string.IsNullOrEmpty(oauthInfo?.client_id))
            {
                _logger.LogError("vincall globalsetting has not connect info");
                return new BadRequestResult();
            }

            if (string.IsNullOrEmpty(model?.Code))
            {
                _logger.LogError("param code is null");
                errMsg = "invalid authorizatoin code";
            }
            else
            {
                try
                {
                    var queryString = HttpContext.Request.QueryString.Value;
                    var originalUrl = queryString.Substring(0, queryString.IndexOf("&code="));
                    _hostProvider.Host = model.Domain;
                    var info = await GetComm100TokenAsync(model.Code, model.SiteId, originalUrl);
                    if (string.IsNullOrEmpty(info?.SiteId) || string.IsNullOrEmpty(info?.AgentId))
                    {
                        throw new UnauthorizedAccessException("comm100 oauth user info");
                    }
                    // ok, get vincall access-token
                    _hostProvider.Host = _Configuration["OauthUri"];
                    var vincallToken = await _vincallClient.GetVincallTokenAsync();
                    var uriRedirect = model.returnUri.BuildUrl(
                        new Dictionary<string, StringValues> {
                        {"code",vincallToken.access_token },
                        {"success","true" },
                    });
                    return Redirect(uriRedirect.ToString());
                }
                catch (Exception ex)
                {
                    errMsg = $"comm100 oauth code :{model.Code},{ex.Message}";
                    _logger.LogError(ex, $"comm100 oauth code :{model.Code},{ex.Message}");
                }
            }

            var uri = model.returnUri.BuildUrl(new Dictionary<string, StringValues> {
                            {"success","false" },
                            {"errMsg", errMsg },
                        });
            return Redirect(uri?.ToString());
        }

        [HttpGet("login/callback")]
        public async Task<IActionResult> CallbackAsync([FromQuery]VincallCallModel model)
        {
            var errMsg = string.Empty;
            var oauthInfo = await _settingService.GetAsync("vincall");
            if (string.IsNullOrEmpty(oauthInfo?.client_id))
            {
                return new BadRequestResult();
            }

            if (string.IsNullOrEmpty(model?.Code))
            {
                errMsg = "invalid authorizatoin code";
            }
            else
            {
                try
                {
                    var queryString = HttpContext.Request.QueryString.Value;
                    var originalUrl = queryString.Substring(0, queryString.IndexOf("&code="));
                    _hostProvider.Host = model.Domain;
                    var info = await GetTokenAsync(model.Code, originalUrl);

                    await UpdateConnectionAsync(model.SiteId);


                    var uriRedirect = SSoController.BuildUrl(model.redirect_uri, new Dictionary<string, StringValues> {
                        {"code",info }
                    });

                    _logger.LogInformation("access_token----->" + info);
                    return Redirect(uriRedirect.ToString());
                }
                catch (Exception ex)
                {
                    errMsg = $"vincall oauth code :{model.Code},{ex.Message}";
                    _logger.LogError(ex, $"vincall oauth code :{model.Code},{ex.Message}");
                }
            }

            var uri = SSoController.BuildUrl(oauthInfo.redirect_uri, new Dictionary<string, StringValues> {
                            {"success","false" },
                            {"errMsg", errMsg },
                        });
            return Redirect(uri?.ToString());
        }

        private async Task<bool> CheckIfConnectionStateTrueAsync(int siteId)
        {
            var connectionState = await _services.ReadSingleAsync<ConnectionState>(siteId);
            if (connectionState?.Connected == true)
            {
                return true;
            }
            return false;
        } 

        private async Task<Comm100Info> GetComm100TokenAsync(string code, string siteId, string queryString)
        {
            var info = await _settingService.GetAsync("connect");
            info.grant_type = info.grant_type ?? "authorization_code";
            var tokenResult = await _comm100OauthClient.QueryAccessTokenAsync(siteId, code, info.client_id, info.client_secret, info.redirect_uri + queryString, info.grant_type);
            return await _comm100OauthClient.GetProfileInfoAsync($"{tokenResult.token_type} {tokenResult.access_token}", siteId);
        }


        private async Task UpdateConnectionAsync(int siteId)
        {
            var connectionState = await _services.ReadSingleAsync<ConnectionState>(siteId);
            connectionState.Connected = true;
            await _services.UpdateAndSaveAsync<ConnectionState>(connectionState);
        }


        private async Task<string> GetTokenAsync(string code, string queryString)
        {
            var info = await _settingService.GetAsync("vincall");
            var tokenResult = (await _comm100OauthClient.VincallQueryAccessTokenAsync(code, info.client_id, info.client_secret,info.redirect_uri+queryString, info.grant_type))?.access_token??string.Empty;
            return tokenResult;
        }
    }
}
