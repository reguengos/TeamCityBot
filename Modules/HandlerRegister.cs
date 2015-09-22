using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelloBotCommunication;

namespace Modules
{
	public class HandlerRegister : IActionHandlerRegister
	{
		public List<IActionHandler> GetHandlers(IDictionary<string, string> moduleParameters, IEnumerable<ITeamCityBuildChecker> buildCheckers)
		{
            string jiraLogin;
		    string jiraPassword;
		    string jiraAddress;
		    moduleParameters.TryGetValue("jira-login", out jiraLogin);
		    moduleParameters.TryGetValue("jira-password", out jiraPassword);
		    moduleParameters.TryGetValue("jira-address", out jiraAddress);

			return new List<IActionHandler>
			{
			    new RuOkHandler(),
                new JiraIssueHandler(jiraAddress,jiraLogin,jiraPassword),
				new MuteHandler(buildCheckers),
				new UnmuteHandler(buildCheckers),
				new BlameHandler(buildCheckers)
			};
		}
	}
}
