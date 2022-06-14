using System;
using System.Collections.Generic;
using System.Text;

namespace VincallIntegration.Infrastructure
{
    public partial class Agent
    {
        public int Id { get; set; }
        public string DeviceNumber { get; set; }
        public string UserAccount { get; set; }
        public string Remark { get; set; }
        public int? State { get; set; }//0:offline  1:avaliable  2: oncall   3:do not disturb
        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
