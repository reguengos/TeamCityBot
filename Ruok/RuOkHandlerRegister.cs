using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelloBotCommunication;

namespace Ruok
{
	public class RuOkHandlerRegister : IActionHandlerRegister
	{
		public List<IActionHandler> GetHandlers()
		{
			return new List<IActionHandler> {new RuOkHandler()};
		}
	}
}
