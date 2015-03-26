using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using TeamCityBot;
using TeamCitySharp;
using TeamCitySharp.ActionTypes;
using TeamCitySharp.DomainEntities;

namespace Tests
{
    public class TeamCityBotTests
    {
        [Test]
        public void TeamCityBot_Starts_And_Stops()
        {
            var skype = Substitute.For<ISkypeAdapter>();
            var tc = Substitute.For<ITeamCityClient>();
            var chat = Substitute.For<IChat>();
            skype.GetChat("").ReturnsForAnyArgs(chat);
            var bot = new TeamCityBot.TeamCityBot();
            var botParameters = new BotParameters { PublishChatName = "test" };
            bot.StartBot(skype, tc, botParameters, new Dictionary<string, string>());
            
            Thread.Sleep(3000);

            skype.OnMessageReceived += Raise.Event<EventHandler<SkypeMessageReceivedEventArgs>>(skype,
                new SkypeMessageReceivedEventArgs("!ruok", "test user", chat));

            Thread.Sleep(1000);
            chat.Received().SendMessage(Arg.Is<string>(x => x.StartsWith("imok")));

            bot.Stop();
        }

        [Test]
        public void TeamCityBot_BuildFailed_MessageIsPosted()
        {
            var skype = Substitute.For<ISkypeAdapter>();
            var tc = Substitute.For<ITeamCityClient>();
            var chat = Substitute.For<IChat>();
            skype.GetChat("").ReturnsForAnyArgs(chat);
            var bot = new TeamCityBot.TeamCityBot();
            var botParameters = new BotParameters
            {
                PublishChatName = "test",
                Branches = new []{"dev"}
            };

            var builds = Substitute.For<IBuilds>();
            var build = new Build() { Id = "1", Number = "1", Status = "FAILED" };
            builds.ByBuildLocator(null).ReturnsForAnyArgs(new List<Build> { build });
            var changes = Substitute.For<IChanges>();
            var change = new Change { Username = "Joe" };
            changes.ByLocator(null).ReturnsForAnyArgs(new List<Change> { change });
            tc.Builds.Returns(builds);
            tc.Changes.Returns(changes);

            bot.StartBot(skype, tc, botParameters, new Dictionary<string, string>(), 10);

            Thread.Sleep(20000);

            chat.ReceivedWithAnyArgs().SendMessage("Build 1 (dev) is broken by: Joe");
        }

        [Test]
        public void TeamCityBot_BuildFailed_BecauseOfTests()
        {
            var skype = Substitute.For<ISkypeAdapter>();
            var tc = Substitute.For<ITeamCityClient>();
            var chat = Substitute.For<IChat>();
            skype.GetChat("").ReturnsForAnyArgs(chat);
            var bot = new TeamCityBot.TeamCityBot();
            var botParameters = new BotParameters
            {
                PublishChatName = "test",
                Branches = new[] { "dev" }
            };

            var builds = Substitute.For<IBuilds>();
            var build = new Build() { Id = "1", Number = "1", Status = "FAILED", StatusText = "Tests failed: 25"};
            builds.ByBuildLocator(null).ReturnsForAnyArgs(new List<Build> { build });
            builds.ByBuildId(null).ReturnsForAnyArgs(build);
            var changes = Substitute.For<IChanges>();
            var change = new Change { Username = "Joe" };
            changes.ByLocator(null).ReturnsForAnyArgs(new List<Change> { change });
            tc.Builds.Returns(builds);
            tc.Changes.Returns(changes);
            var testOccurrences = Substitute.For<ITestOccurrences>();
            var testOccurrence = new TestOccurrence() {Name = "Assembly: TestName "};
            testOccurrences.ByBuildId(Arg.Any<string>(), Arg.Any<int>()).ReturnsForAnyArgs(new List<TestOccurrence> { testOccurrence });
            tc.TestOccurrences.Returns(testOccurrences);

            bot.StartBot(skype, tc, botParameters, new Dictionary<string, string>(), 10);

            Thread.Sleep(20000);

            chat.ReceivedWithAnyArgs().SendMessage("Build 1 (dev) is broken by: Joe");
        }
    }
}
