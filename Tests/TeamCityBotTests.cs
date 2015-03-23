using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using TeamCityBot;

namespace Tests
{
    public class TeamCityBotTests
    {
        [Test]
        public void TeamCityBot_Starts_And_Stops()
        {
            var skype = Substitute.For<ISkypeAdapter>();
            var chat = Substitute.For<IChat>();
            skype.GetChat("").ReturnsForAnyArgs(chat);
            var bot = new TeamCityBot.TeamCityBot();
            var botParameters = new BotParameters { PublishChatName = "test" };
            bot.StartBot(skype, botParameters, new Dictionary<string, string>());
            
            Thread.Sleep(3000);

            skype.OnMessageReceived += Raise.Event<EventHandler<SkypeMessageReceivedEventArgs>>(skype,
                new SkypeMessageReceivedEventArgs("!ruok", "test user", chat));

            Thread.Sleep(1000);
            chat.Received().SendMessage(Arg.Is<string>(x => x.StartsWith("imok")));

            bot.Stop();
        }
    }
}
