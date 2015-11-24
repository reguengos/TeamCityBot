using System;
using System.Collections.Generic;


namespace HelloBotCommunication
{
    public interface IActionHandlerRegister
    {
	    List<IActionHandler> GetHandlers(IDictionary<string, string> moduleParameters,
		    IEnumerable<ITeamCityBuildChecker> buildCheckers, Action<string> sendCommand);
    }
}
