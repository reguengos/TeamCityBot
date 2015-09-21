using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelloBotCommunication;

namespace Modules
{
	class MuteHandler : IActionHandler
	{
		private readonly IEnumerable<ITeamCityBuildChecker> _checkers;
		public List<string> CallCommandList { get{return new List<string> { "mute" };} }
		public string CommandDescription { get; private set; }

		public MuteHandler(IEnumerable<ITeamCityBuildChecker> checkers)
		{
			_checkers = checkers;
		}

		public void HandleMessage(string args, object clientData, Action<string> sendMessageFunc)
		{
			var checker =
				_checkers.FirstOrDefault(
					x => string.Equals(x.Branch, args, StringComparison.InvariantCultureIgnoreCase));
			if (checker != null)
			{
				checker.Mute();
			}
		}
	}
}
