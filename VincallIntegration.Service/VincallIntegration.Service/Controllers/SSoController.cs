using AutoMapper;
using IdentityModel;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VincallIntegration.Infrastructure;
using VincallIntegration.Service.Models;
using VincallIntegration.Service.WebApiServices;
using VincallIntegration.Application;
using VincallIntegration.Application.Dtos;

namespace VincallIntegration.Service.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class SSoController : ControllerBase
    {
        private readonly ICrudServices _services;
        private readonly IMapper _mapper;
        private readonly IComm100OauthClient _comm100OauthClient;

        private readonly GlobalSettingService _settingService;
        private readonly IConfiguration _config;
        private readonly HostProvider _hostProvider;
        private readonly ILogger<SSoController> _logger;

        public SSoController(ICrudServices services, IMapper mapper, IComm100OauthClient comm100OauthClient, GlobalSettingService settingService,IConfiguration config, HostProvider hostProvider, ILogger<SSoController> logger)
        {
            _services = services;
            _mapper = mapper;
            _comm100OauthClient = comm100OauthClient;
            _settingService = settingService;
            _config = config;
            _hostProvider = hostProvider;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> LoginAsync(SsoModel model)
        {
            var user = HttpContext.User;

            var account = user.Claims.FirstOrDefault(x => x.Type == "UserAccount")?.Value;
            var userComm100 = await _services.ReadSingleAsync<UserComm100>(x => x.Account == account);
            if (userComm100 == null)
            {
                return BadRequest($"No Bind user for account: {account}");
            }
            //create jwt by usr
            var token = CreataToken(userComm100);
            //redirect comm100 access_token

            var uri = BuildUrl(model.ReturnUrl, new Dictionary<string, StringValues>
            {
                {"token","token" }
            });
            return Redirect(uri.ToString());


        }

        /// <summary>
        /// agentid comes from my db
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> JwtSignInAsync([FromQuery]JwtModel model)
        {
            var user = HttpContext.User;
            var account = user.Claims.FirstOrDefault(x => x.Type == "UserAccount")?.Value;
            var userComm100 = await _services.ReadSingleAsync<UserComm100>(x => x.Account == account);
            if (userComm100 == null)
            {
                return BadRequest($"No Bind user for account: {account}");
            }
            //create jwt by usr
            var token = CreataToken(userComm100);
            //redirect comm100 access_token

            var uri = BuildUrl(model.redirect_url, new Dictionary<string, StringValues>
            {
                {"jwt",token},
                {"relayState",model.RelayState }
            });
            return Redirect(uri.ToString());


        }
        public static Uri BuildUrl(string url, IDictionary<string, StringValues> parameters)
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

        /// <summary>
        ///  login vincall ,oauth callback,  //302
        ///  write cookie
        /// </summary>
        /// <returns></returns>

        [HttpGet]     
        public async Task<IActionResult> CallbackAsync([FromQuery]SsoCallModel model)
        {
            var errMsg = string.Empty;
            var oauthInfo = await _settingService.GetAsync("comm100");
            if(string.IsNullOrEmpty(oauthInfo?.client_id))
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
                    _hostProvider.Host = model.Domain;    
                    var info = await GetTokenAsync(model.Code, model.SiteId);

                    //find user
                    var userComm100 = await _services.ReadSingleAsync<UserComm100>(x => x.SiteId == info.SiteId && x.ExternId == info.AgentId);
                    if (userComm100 == null)
                    {
                        //create agent
                        var vincallUser = await _services.CreateAndSaveAsync<User>(new User
                        {
                            UserName = info.AgentId,
                            Account = info.Email ?? "comm100user",
                            IsAdmin = false,
                            Password = Md5Helper.Md5("Aa000000")
                        });
                        await _services.CreateAndSaveAsync<UserComm100>(new UserComm100
                        {
                            Account = vincallUser.Account,
                            Email = info.Email,
                            ExternId = info.AgentId,
                            PartnerId = info.PartnerId,
                            SiteId = info.SiteId,
                        });

                        await WriteCookieAsync(HttpContext, vincallUser);
                        var uriRedirect = BuildUrl(oauthInfo.redirect_uri, new Dictionary<string, StringValues> {
                            {"userId",vincallUser.Id.ToString() },
                            {"role", "user" },
                            {"userAccount",vincallUser.Account },
                            {"userName",vincallUser.UserName },
                            {"success","true" },
                        });
                        return Redirect(uriRedirect.ToString());
                    }
                    else
                    {
                        var user = await _services.ReadSingleAsync<User>(x => x.Account == userComm100.Account);
                        if (user == null)
                        {
                            errMsg=$"No  user for account: {userComm100.Account}";
                        }
                        await WriteCookieAsync(HttpContext, user);
                        var uriRedirect = BuildUrl(oauthInfo.redirect_uri, new Dictionary<string, StringValues> {
                            {"userId",user.Id.ToString() },
                            {"role", "user" },
                            {"userAccount",user.Account },
                            {"userName",user.UserName },
                            {"success","true" },
                        });
                        return Redirect(uriRedirect.ToString());
                    }
                    
                    
                }
                catch(Exception ex)
                {
                    errMsg = $"comm100 oauth code :{model.Code},{ex.Message}";
                    _logger.LogError(ex, $"comm100 oauth code :{model.Code},{ex.Message}");
                }
            }

            var uri = BuildUrl(oauthInfo.redirect_uri, new Dictionary<string, StringValues> {
                            {"success","false" },
                            {"errMsg", errMsg },
                        });
            return Redirect(uri?.ToString());
        }


        public static async Task<VincallToken> WriteCookieAsync(HttpContext context, User user)
        {

            var userId = user.Id.ToString();
            var role = user.IsAdmin ? "admin" : "user";
            var userName = user.UserName;
            var account = user.Account;
            var isu = new IdentityServerUser("vincall")
            {
                IdentityProvider = IdentityServerConstants.LocalIdentityProvider,
                AuthenticationTime = DateTime.UtcNow
            };
            isu.AuthenticationMethods.Add(OidcConstants.AuthenticationMethods.Password);
            isu.AdditionalClaims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            isu.AdditionalClaims.Add(new Claim(ClaimTypes.Role, role));
            isu.AdditionalClaims.Add(new Claim(ClaimTypes.Name, userName));
            isu.AdditionalClaims.Add(new Claim("UserAccount", account));
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, isu.CreatePrincipal());

            var tokenResult = new VincallToken()
            {
                access_token = "test",
                refresh_token = "test",

            };
            tokenResult.userId = userId;
            tokenResult.role = role;
            tokenResult.userName = userName;
            tokenResult.userAccount = account;
            return tokenResult;
        }


        private async Task<Comm100Info> GetTokenAsync(string code,string siteId)
        {
            var info = await _settingService.GetAsync("comm100");
            var tokenResult = await _comm100OauthClient.QueryAccessTokenAsync(siteId, code, info.client_id, info.client_secret, info.redirect_uri,info.grant_type);
            return await _comm100OauthClient.GetProfileInfoAsync($"{tokenResult.token_type} {tokenResult.access_token}", siteId);  
        }

        private string CreataToken( UserComm100 context)
        {
           
            var privateKey = _config["TokenPrivateKey"];
            RSA rsa = RSA.Create();
            rsa.ImportEncryptedPkcs8PrivateKey(Encoding.UTF8.GetBytes("vincall"), Convert.FromBase64String(privateKey), out _);
            var securityKey = new RsaSecurityKey(rsa);

            List<Claim> authClaims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, context.Account),
                new Claim("AgentId", context.ExternId),
                new Claim("partnerId", context.PartnerId),
                new Claim(JwtClaimTypes.Email, context.Email),
                new Claim("siteId",context.SiteId),
                new Claim(ClaimTypes.NameIdentifier,context.Email)
            };
            var token = new JwtSecurityToken(  "auth", "vincall", authClaims, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(10), new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256));
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;

        }

    }
}
