using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VincallIntegration.Application;
using VincallIntegration.Application.Dtos;
using VincallIntegration.Infrastructure;

namespace VincallIntegration.Service.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ConnectStateController : ControllerBase
    {
        private readonly ICrudServices _services;
        private readonly IMapper _mapper;
        public ConnectStateController(ICrudServices services, IMapper mapper)
        {
            _services = services;
            _mapper = mapper;
        }


        [Authorize]
        [HttpGet]
        public async Task<ConnectionStateDto> Get(int siteId)
        {
            var connectionState = await _services.ReadSingleAsync<ConnectionState>(item=>item.SiteId==siteId);

            var result = _mapper.Map<ConnectionStateDto>(connectionState);

            return await Task.FromResult<ConnectionStateDto>(result);
        }

        [Authorize]
        [HttpPut("/disconnect")]
        public async Task<ConnectionStateDto> Set(int siteId)
        {
            var connectionState = await _services.ReadSingleAsync<ConnectionState>(item => item.SiteId == siteId);

            if (connectionState != null)
            {
                connectionState.Connected = false;

                await _services.UpdateAndSaveAsync<ConnectionState>(connectionState);
            }
            
            var connectionStateNew = await _services.ReadSingleAsync<ConnectionState>(item => item.SiteId == siteId);

            var result = _mapper.Map<ConnectionStateDto>(connectionStateNew);

            return await Task.FromResult<ConnectionStateDto>(result);
        }
    }
}
