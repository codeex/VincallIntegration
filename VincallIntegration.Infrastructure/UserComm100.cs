using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VincallIntegration.Infrastructure
{
    public class UserComm100
    {
        [Key]
        public long Id { get; set; }
        public string Account { get; set; }
        public string ExternId { get; set; }
        public string SiteId { get; set; }
        public string PartnerId { get; set; }
        public string Email { get; set; }

        public UserComm100(string comm100AgentId, string vincallAgentId, string siteId, string partnerId, string email)
        {
            Account = vincallAgentId;
            ExternId = comm100AgentId;
            SiteId = siteId;
            PartnerId = partnerId;
            Email = email;
        }

        public UserComm100()
        {

        }
    }
}
