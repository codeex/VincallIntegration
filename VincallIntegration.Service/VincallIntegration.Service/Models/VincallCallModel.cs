using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VincallIntegration.Service.Models
{
    public class VincallCallModel
    {
        public string Code { get; set; }//
        public string AgentId { get; set; }
        public int SiteId { get; set; }//
        public string Domain { get; set; }//
        public string Scope { get; set; }
        public string Token { get; internal set; }

        public string redirect_uri { get; set; }
    }
}
