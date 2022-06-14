using System;
using System.Collections.Generic;
using System.Text;

namespace VincallIntegration.Application.Dtos
{
    public class AgentDto
    {
        public int Id { get; set; }
        public string DeviceNumber { get; set; }
        public string UserAccount { get; set; }
        public string Remark { get; set; }      
        public DateTime? CreateDate { get; set; }
    }

    public class AgentWithoutIdDto
    {
        public string DeviceNumber { get; set; }
        public string Remark { get; set; }

    }

    public class AgentResult
    {
        public int Count { get; set; }

        public List<AgentDto> Agents { get; set; }
    }

}
