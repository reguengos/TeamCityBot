using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SKYPE4COMLib;
using TeamCitySharp;

namespace TeamCityBot
{
    internal class Program
    {
        private static readonly TeamCityBot TeamCityBot = new TeamCityBot();

        private static void Main(string[] args)
        {
            if (args.Length < 5)
            {
                Console.WriteLine("Usage: <chat name> <TC server> <login> <password> <build config id>");
                return;
            }

            var botParameters = new BotParameters
            {
                PublishChatName = args[0],
                TeamCityServer = args[1],
                TeamCityLogin = args[2],
                TeamCityPassword = args[3],
                BuildConfigId = args[4],
                Branches = args[5].Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
            };

            var moduleParameters = new Dictionary<string, string>();

            foreach (var arg in args.Skip(6))
            {
                var p = arg.Split(':');
                if (p.Length == 2)
                {
                    moduleParameters[p[0]] = p[1];
                }
            }

            var skype = new Skype();
            var skypeAdapter = new SkypeAdapter(skype);
            var teamCityClient = new TeamCityClient(botParameters.TeamCityServer);
			teamCityClient.Connect(botParameters.TeamCityLogin, botParameters.TeamCityPassword);

            TeamCityBot.StartBot(skypeAdapter, teamCityClient, botParameters, moduleParameters,
                new TimeConfig
                {
                    BuildCheckInterval = TimeSpan.FromSeconds(15),
                    StillBrokenDelay = TimeSpan.FromMinutes(30)
                });

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}