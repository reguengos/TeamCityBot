using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HelloBotCommunication;

namespace HelloBotModules
{
    public class Test : IActionHandler
    {
        public List<string> CallCommandList
        {
            get { return new List<string> { "test"}; }
        }

        public string CommandDescription
        {
            get { return "Test"; }
        }

        public void HandleMessage(string args, object clientData, Action<string> sendMessageFunc)
        {
            sendMessageFunc("test ok");
        }
    }

    public class HandlerRegister : IActionHandlerRegister
    {
        public List<IActionHandler> GetHandlers()
        {
            return new List<IActionHandler>
            {
                new Test(),
                new Boobs()
            };
        }
    }

}
