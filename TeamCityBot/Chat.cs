using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SKYPE4COMLib;

namespace TeamCityBot
{
    public class SkypeChat : IChat
    {
        private Chat _chat;

        public SkypeChat(Chat chat)
        {
            _chat = chat ;
        }

        public void SendMessage(string message)
        {
            _chat.SendMessage(message);
        }
    }
}
