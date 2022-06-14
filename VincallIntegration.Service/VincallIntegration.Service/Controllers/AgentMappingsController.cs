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
    public class AgentMappingsController : ControllerBase
    {
        private readonly ICrudServices _services;
        private readonly IMapper _mapper;
        public AgentMappingsController(ICrudServices services, IMapper mapper)
        {
            _services = services;
            _mapper = mapper;
        }


        [Authorize]
        [HttpGet]
        public async Task<IEnumerable<AgentMappingDto>> GetAll()
        {
            var accounts = _services.ReadManyNoTracked<User>().Select(item => item.Account);

            var agentMappings = _services.ReadManyNoTracked<UserComm100>(item => accounts.Contains(item.Account));

            var result = _mapper.Map<List<AgentMappingDto>>(agentMappings.ToList());

            return await Task.FromResult<IEnumerable<AgentMappingDto>>(result);
        }

        [Authorize]
        [HttpPut]
        public async Task<IEnumerable<AgentMappingDto>> Update([FromBody]IEnumerable<AgentMappingDto> agentMappings)
        {
            var sources = _mapper.Map<List<UserComm100>>(agentMappings.ToList());

            var userComm100es = await _services.DeleteAllThenInsertAndSaveAsync(sources);

            var result = _mapper.Map<List<AgentMappingDto>>(userComm100es.ToList());

            return await Task.FromResult<IEnumerable<AgentMappingDto>>(result);
        }
    }
}
