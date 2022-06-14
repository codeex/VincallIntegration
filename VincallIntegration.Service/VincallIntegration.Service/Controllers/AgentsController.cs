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
    public class AgentsController : ControllerBase
    {
        private readonly ICrudServices _services;
        private readonly IMapper _mapper;
        public AgentsController(ICrudServices services, IMapper mapper)
        {
            _services = services;
            _mapper = mapper;
        }       

        [Authorize]
        [HttpGet()]
        public async Task<AgentResult> QueryAgentsAsync(string keywords, int pageSize = 0, int pageNum = 0)
        {
            var result = new AgentResult();

            var agents = _services.ReadManyNoTracked<Agent>();
            if (!string.IsNullOrEmpty(keywords))
            {
                agents = agents.Where(x => x.UserAccount.Contains(keywords));
            }
            result.Count = agents.Count();
            if (pageSize != 0 || pageNum != 0)
            {
                agents = agents.Page<Agent>(pageNum, pageSize);
            }
            result.Agents = _mapper.Map<List<AgentDto>>(agents.ToList());
            return await Task.FromResult<AgentResult>(result);
        }


    }
}
