using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using SKYPE4COMLib;
using TeamCityBot;

namespace Tests
{
    public class TeamCityBotTests
    {
        [Test]
        public void TeamCityBot_Starts_And_Stops()
        {
            var skype = Substitute.For<Skype>();
            var bot = new TeamCityBot.TeamCityBot();
            bot.StartBot(skype, new BotParameters(), new Dictionary<string, string>());

            Thread.Sleep(1000);
            bot.Stop();
        }
    }
}
