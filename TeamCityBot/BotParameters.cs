using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCityBot
{
    public class BotParameters
    {
        public string PublishChatName { get; set; }
        public string TeamCityServer { get; set; }
        public string TeamCityLogin { get; set; }
        public string TeamCityPassword { get; set; }
        public string BuildConfigId { get; set; }
        public string[] Branches { get; set; }

        public BotParameters()
        {
            Branches = new string[0];
        }
    }
}