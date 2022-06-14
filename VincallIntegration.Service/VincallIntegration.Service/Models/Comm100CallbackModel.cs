using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VincallIntegration.Service.Models
{
    public class Comm100CallbackModel
    {
        public string AgentId { get; set; }
        public string SiteId { get; set; }
        public string Domain { get; set; }
        public string returnUri { get; set; }
        public string Code { get; set; }
    }
}
