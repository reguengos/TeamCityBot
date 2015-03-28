using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCityBot
{
    public class TimeConfig
    {
        public TimeSpan BuildCheckInterval { get; set; }
        public TimeSpan StillBrokenDelay { get; set; }

        public TimeConfig()
        {
            BuildCheckInterval = TimeSpan.FromSeconds(15);
            StillBrokenDelay = TimeSpan.FromMinutes(60);
        }
    }
}
