using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comm100Integration.Service.Models
{
    public class JwtModel
    {
        public string RelayState { get; set; }
        public string redirect_url { get; set; }
    }
}
