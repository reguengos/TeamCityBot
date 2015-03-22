using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelloBotCommunication.ClientDataInterfaces;

namespace TeamCityBot
{
    public class SkypeData : ISkypeData
    {
        public string FromName { get; set; }
    }
}
