using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCityBot
{
    public class BuildResultFormatter
    {
        private static readonly Random _r = new Random();
        private readonly List<string> _successEmoji = new List<string> { @"\o/", "(^)", "(sun)", "(clap)", "(party)" };
        private readonly string NEWLINE = Environment.NewLine;

        public string Format(BuildResult value)
        {
            if (value.Status == BuildStatus.Fixed)
            {
                return String.Format(@"{0} Build {1} ({2}) is fixed by: {3}. Nice job!",
                                GetRandomEmoji(_successEmoji), value.Number, value.Branch, value.Author);
            }
            else if (value.Status == BuildStatus.Broken)
            {
                return String.Format("Build {0} ({1}) is broken by: {2}{5}{3}{5}{4}{5}{6}",
                    value.Number,
                    value.Branch,
                    value.Author,
                    value.ReasonText,
                    value.WebUrl,
                    NEWLINE,
                    value.DetailedReason);
            }
            else if (value.Status == BuildStatus.StillBroken)
            {
                return String.Format("Build {0} ({1}) is still broken by: {2}{5}{3}{5}{4}{5}{6}",
                    value.Number,
                    value.Branch,
                    value.Author,
                    value.ReasonText,
                    value.WebUrl,
                    NEWLINE,
                    value.DetailedReason);
            }

            return null;
        }

        private static string GetRandomEmoji(List<string> emojis)
        {
            var i = _r.Next(emojis.Count);
            return emojis[i];
        }
    }
}
