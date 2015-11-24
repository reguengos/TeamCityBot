using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using HelloBotCommunication;

namespace Modules
{
	class AngerHandler : IActionHandler
	{
		private Action<string> _sendMessageFunc;
		public List<string> CallCommandList { get { return new List<string>();} }
		public string CommandDescription { get; private set; }
		private System.Timers.Timer _timer;
		private int lastDay = -1;

		public AngerHandler(Action<string> sendMessageFunc)
		{
			_sendMessageFunc = sendMessageFunc;
			_timer = new Timer(20000);
			_timer.Elapsed += _timer_Elapsed;
			_timer.Start();
		}

		public void HandleMessage(string args, object clientData, Action<string> sendMessageFunc)
		{
		}

		void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			var now = DateTime.Now;
			if (now.Hour == 12 && now.Minute == 0 && (now.Day != lastDay || lastDay == -1) 
				&& now.DayOfWeek != DayOfWeek.Saturday && now.DayOfWeek != DayOfWeek.Sunday)
			{
				_sendMessageFunc("(anger)");
				lastDay = now.Day;
			}
		}
	}
}
