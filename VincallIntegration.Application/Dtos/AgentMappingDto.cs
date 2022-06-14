using System;
using System.Collections.Generic;
using System.Text;

namespace VincallIntegration.Application.Dtos
{
    public class AgentMappingDto
    {
        public string Comm100AgentId { get; set; }

        public string VincallAgentId { get; set; }

        public string SiteId { get; set; }

        public string PartnerId { get; set; }

        public string Email { get; set; }

        public AgentMappingDto(string externId, string account, string siteId, string partnerId, string email)
        {
            Comm100AgentId = externId;
            VincallAgentId = account;
            SiteId = siteId;
            PartnerId = partnerId;
            Email = email;
        }

        public AgentMappingDto()
        {

        }
    }
}
