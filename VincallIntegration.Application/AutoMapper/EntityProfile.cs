using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VincallIntegration.Application.Dtos;
using VincallIntegration.Infrastructure;

namespace VincallIntegration.Application.AutoMapper
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
