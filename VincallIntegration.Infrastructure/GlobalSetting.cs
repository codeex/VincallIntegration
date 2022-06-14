using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VincallIntegration.Infrastructure
{
    public class GlobalSetting
    {
        [Key]
        public long Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
    }
}
