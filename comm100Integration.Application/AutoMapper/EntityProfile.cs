using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using Comm100Integration.Application.Dtos;
using Comm100Integration.Infrastructure;

namespace Comm100Integration.Application.AutoMapper
{
    public class EntityProfile: Profile
    {
        public EntityProfile()
        {
            CreateMap<UserComm100, AgentMappingDto>();
            CreateMap<AgentMappingDto, UserComm100>();
            CreateMap<ConnectionState, ConnectionStateDto>();
            CreateMap<Agent, AgentDto>();
        }
    }
}
