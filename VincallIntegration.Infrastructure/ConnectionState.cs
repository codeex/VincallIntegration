using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VincallIntegration.Infrastructure
{
    public class ConnectionState
    {
        [Key]
        public int SiteId { get; set; }

        public bool Connected { get; set; }

        public string Server { get; set; }
    }
}
