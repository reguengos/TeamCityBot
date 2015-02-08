using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HelloBotCommunication
{
    public interface IRegexActionHandler : IActionHandler
    {
        List<Regex> CallRegexes { get; }
    }
}
