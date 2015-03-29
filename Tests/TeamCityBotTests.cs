using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using TeamCityBot;
using TeamCitySharp;
using TeamCitySharp.ActionTypes;
using TeamCitySharp.DomainEntities;
using TeamCitySharp.Locators;

namespace Tests
{
    public class TeamCityBotTests
    {
        private ISkypeAdapter skype;
        private ITeamCityClient tc;
        private IChat chat;
        private TeamCityBot.TeamCityBot bot;
        private BotParameters botParameters;
        private FakeBuilds builds;

        [SetUp]
        public void SetUp()
        {
            Console.WriteLine("Setup start");
            skype = Substitute.For<ISkypeAdapter>();
            tc = Substitute.For<ITeamCityClient>();
            chat = Substitute.For<IChat>();
            skype.GetChat("").ReturnsForAnyArgs(chat);
            bot = new TeamCityBot.TeamCityBot();
            botParameters = new BotParameters
            {
                PublishChatName = "test",
                Branches = new[] { "dev" }
            };
            builds = new FakeBuilds();
            tc.Builds.Returns(builds);
            Console.WriteLine("Setup finish");
        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine("Teardown start");
            bot.Stop();
            this.bot = null;
            this.botParameters = null;
            this.chat.ClearReceivedCalls();
            this.chat = null;
            this.skype = null;
            this.tc = null;
            Console.WriteLine("Teardown finish");
        }

        [Test]
        public async Task TeamCityBot_StartsAndStops()
        {
            bot.StartBot(skype, tc, botParameters, new Dictionary<string, string>(), new TimeConfig { BuildCheckInterval = TimeSpan.FromMilliseconds(10) });
            Thread.Sleep(1000);

            skype.OnMessageReceived += Raise.Event<EventHandler<SkypeMessageReceivedEventArgs>>(skype,
                new SkypeMessageReceivedEventArgs("!ruok", "test user", chat));

            Thread.Sleep(1000);
            chat.Received().SendMessage(Arg.Is<string>(x => x.StartsWith("imok")));

            bot.Stop();
        }
    }
}

