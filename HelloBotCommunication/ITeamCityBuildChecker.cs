using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloBotCommunication
{
	public interface ITeamCityBuildChecker
	{
		string Branch { get; }

		void Mute();
		void Unmute();

		void Blame(string person);
	}
}
