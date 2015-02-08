using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HelloBotCommunication;

namespace Modules
{
	public class RuOkHandler : IActionHandler
    {
		public List<string> CallCommandList
		{
			get { return new List<string> { "ruok" }; }
		}

		public string CommandDescription
		{
			get { return "checks if bot is working"; }
		}

		public void HandleMessage(string args, object clientData, Action<string> sendMessageFunc)
		{
			sendMessageFunc("imok " + args);
		}
	}
}
