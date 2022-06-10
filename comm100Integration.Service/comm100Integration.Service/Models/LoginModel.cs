using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comm100Integration.Service.Models
{
    public class LoginModel
    {
        public string AgentId { get; set; }
        public string SiteId { get; set; }
        public string Domain { get; set; }
        public string returnUri { get; set; }
    }
}
