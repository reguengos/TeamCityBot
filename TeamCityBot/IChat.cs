using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCityBot
{
    public interface IChat
    {
        void SendMessage(string message);
    }
}
