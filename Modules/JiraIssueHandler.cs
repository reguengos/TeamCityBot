using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HelloBotCommunication;
using Newtonsoft.Json;

namespace Modules
{
    public class JiraIssueHandler : IRegexActionHandler
    {
        private string _login;
        private string _password;
        private string _jiraAddress;

        public JiraIssueHandler(string jiraAddress, string login, string password)
        {
            _login = login;
            _password = password;
            _jiraAddress = jiraAddress;
        }

        public List<System.Text.RegularExpressions.Regex> CallRegexes
        {
            get
            {
                return new List<Regex>()
                {
                    new Regex("RDC-[0-9]{0,6}", RegexOptions.Compiled)
                };
            }
        }

        public List<string> CallCommandList
        {
            get { return new List<string>(); }
        }

        public string CommandDescription
        {
            get { return "post jira issue description"; }
        }

        public void HandleMessage(string args, object clientData, Action<string> sendMessageFunc)
        {
            var issue = CallRegexes[0].Match(args).Groups[0].Value;

            if (!_jiraAddress.StartsWith("http://"))
            {
                _jiraAddress = "http://" + _jiraAddress;
            }

            var hostUri = new Uri(_jiraAddress);
            var restIssueLink = new Uri(hostUri,  "/rest/api/latest/issue/" + issue);

            WebRequest request = (HttpWebRequest)WebRequest.Create(restIssueLink);
            String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(_login + ":" + _password));
            request.Headers.Add("Authorization", "Basic " + encoded);
            request.PreAuthenticate = true;
            var response = request.GetResponse();
            var text = new StreamReader(response.GetResponseStream()).ReadToEnd();

            var obj = JsonConvert.DeserializeObject(text) as dynamic;

            var sb = new StringBuilder();
            sb.AppendLine(issue + ":");
            sb.AppendLine(obj.fields.summary.Value as string);
            sb.AppendLine(ToShort(obj.fields.description.Value as string, 500));
            sb.AppendLine();
            sb.AppendLine("Автор: " + obj.fields.creator.displayName as string);
            sb.AppendLine("Исполнитель: " + obj.fields.assignee.displayName as string);

            var link = String.Format("https://jira.egspace.ru/browse/{0}", issue);

            sb.AppendLine(link);

            sendMessageFunc(sb.ToString());
        }

        private string ToShort(string input, int num)
        {
            return input.Length <= num ? input : input.Substring(0, num) + "...";
        }
    }
}
