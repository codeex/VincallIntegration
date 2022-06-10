using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comm100Integration.Service.Models
{
    public class SsoModel
    {
        public string AppId { get; set; }
        public int SiteId { get; set; }
        public string ReturnUrl { get; set; }
    }
}
