using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelloBotCommunication;

namespace Modules
{
	class BlameHandler : IActionHandler
	{
		private readonly IEnumerable<ITeamCityBuildChecker> _checkers;
		public List<string> CallCommandList { get { return new List<string> { "blame" }; } }
		public string CommandDescription { get; private set; }

		public BlameHandler(IEnumerable<ITeamCityBuildChecker> checkers)
		{
			_checkers = checkers;
		}

		public void HandleMessage(string args, object clientData, Action<string> sendMessageFunc)
		{
			var arr = args.Split(new char[] {' '}, 2);
			var branch = arr[0];
			var person = arr[1];
			var checker =
				_checkers.FirstOrDefault(
					x => string.Equals(x.Branch, branch, StringComparison.InvariantCultureIgnoreCase));
			if (checker != null)
			{
				checker.Blame(person);
			}
		}
	}
}
