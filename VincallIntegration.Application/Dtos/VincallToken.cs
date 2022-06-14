using System;
using System.Collections.Generic;
using System.Text;

namespace VincallIntegration.Application.Dtos
{
    public class VincallToken
    {
        public string access_token { get; set; }

        public string refresh_token { get; set; }


        public string userId { get; set; }

        public string userName { get; set; }

        public string userAccount { get; set; }
        public string role { get; set; }
    }
}
