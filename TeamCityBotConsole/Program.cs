using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SKYPE4COMLib;

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
                BuildConfigId = args[4]
            };

            var moduleParameters = new Dictionary<string, string>();

            foreach (var arg in args.Skip(5))
            {
                var p = arg.Split(':');
                if (p.Length == 2)
                {
                    moduleParameters[p[0]] = p[1];
                }
            }

            var skype = new Skype();
            var skypeAdapter = new SkypeAdapter(skype);

            TeamCityBot.StartBot(skypeAdapter, botParameters, moduleParameters);

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}