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

        [Test]
        public async Task TeamCityBot_BuildFailed_MessageIsPosted()
        {
            builds.Add(new Build() { Id = "1", Number = "1", Status = "FAILED", StatusText = "build error" });
            var changes = Substitute.For<IChanges>();
            var change = new Change { Username = "Joe" };
            changes.ByLocator(null).ReturnsForAnyArgs(new List<Change> { change });
            tc.Changes.Returns(changes);
            bot.StartBot(skype, tc, botParameters, new Dictionary<string, string>(), new TimeConfig { BuildCheckInterval = TimeSpan.FromMilliseconds(10)});
            Thread.Sleep(3000);

            chat.Received().SendMessage(Arg.Is<string>(x => x.Contains("Build 1 (dev) is broken by: Joe")));
            bot.Stop();
        }

        //[Test]
        //public async Task TeamCityBot_BuildFailed_BecauseOfTests()
        //{
        //    builds.Add(new Build() { Id = "1", Number = "1", Status = "FAILED", StatusText = "Tests failed: 25"});
        //    var changes = Substitute.For<IChanges>();
        //    var change = new Change { Username = "Joe" };
        //    changes.ByLocator(null).ReturnsForAnyArgs(new List<Change> { change });
        //    tc.Changes.Returns(changes);
        //    var testOccurrences = Substitute.For<ITestOccurrences>();
        //    var testOccurrence = new TestOccurrence() {Name = "Assembly: TestName "};
        //    testOccurrences.ByBuildId(Arg.Any<string>(), Arg.Any<int>()).ReturnsForAnyArgs(new List<TestOccurrence> { testOccurrence });
        //    tc.TestOccurrences.Returns(testOccurrences);
        //    bot.StartBot(skype, tc, botParameters, new Dictionary<string, string>(), new TimeConfig { BuildCheckInterval = TimeSpan.FromMilliseconds(10) });
        //    Thread.Sleep(1000);

        //    chat.Received(1).SendMessage(Arg.Is<string>(x => x.Contains("Build 1 (dev) is broken by: Joe")));
        //    bot.Stop();
        //}

        [Test]
        public async Task TeamCityBot_BuildFailedTwoTimesInARowButBrokenDelayHasNotPassed_SecondMessageIsNotPosted()
        {
            builds.Add(new Build() { Id = "1", Number = "1", Status = "FAILED", StatusText = "Tests failed: 25" });
            builds.Add(new Build() { Id = "2", Number = "2", Status = "FAILED", StatusText = "Tests failed: 25" });
            var changes = Substitute.For<IChanges>();
            var change = new Change { Username = "Joe" };
            changes.ByLocator(null).ReturnsForAnyArgs(new List<Change> { change });
            tc.Changes.Returns(changes);
            var testOccurrences = Substitute.For<ITestOccurrences>();
            var testOccurrence = new TestOccurrence() { Name = "Assembly: TestName " };
            testOccurrences.ByBuildId(Arg.Any<string>(), Arg.Any<int>()).ReturnsForAnyArgs(new List<TestOccurrence> { testOccurrence });
            tc.TestOccurrences.Returns(testOccurrences);
            bot.StartBot(skype, tc, botParameters, new Dictionary<string, string>(), new TimeConfig { BuildCheckInterval = TimeSpan.FromMilliseconds(10), StillBrokenDelay = TimeSpan.FromSeconds(30)});
            Thread.Sleep(50);

            chat.When(x => x.SendMessage(Arg.Any<string>())).Do(c => Console.WriteLine("RECEIVED: " +c.Arg<string>()));
            chat.Received(1).SendMessage(Arg.Is<string>(x => x.Contains("Build 1 (dev) is broken by: Joe")));
            chat.ReceivedWithAnyArgs(1).SendMessage(Arg.Any<string>());
            bot.Stop();
        }

        [Test]
        public async Task TeamCityBot_BuildFailedTwoTimesInARow_SecondMessageIsPostedAfterStillBrokenDelay()
        {
            builds.Add(new Build() { Id = "1", Number = "1", Status = "FAILED", StatusText = "Tests failed: 25" });
            builds.Add(new Build() { Id = "2", Number = "2", Status = "FAILED", StatusText = "Tests failed: 25" });
            var changes = Substitute.For<IChanges>();
            var change = new Change { Username = "Joe" };
            changes.ByLocator(null).ReturnsForAnyArgs(new List<Change> { change });
            tc.Changes.Returns(changes);
            var testOccurrences = Substitute.For<ITestOccurrences>();
            var testOccurrence = new TestOccurrence() { Name = "Assembly: TestName " };
            testOccurrences.ByBuildId(Arg.Any<string>(), Arg.Any<int>()).ReturnsForAnyArgs(new List<TestOccurrence> { testOccurrence });
            tc.TestOccurrences.Returns(testOccurrences);
            bot.StartBot(skype, tc, botParameters, new Dictionary<string, string>(), new TimeConfig { BuildCheckInterval = TimeSpan.FromMilliseconds(10), StillBrokenDelay = TimeSpan.FromMilliseconds(100) });
            Thread.Sleep(1000);

            chat.Received(1).SendMessage(Arg.Is<string>(x => x.Contains("Build 1 (dev) is broken by: Joe")));
            chat.Received(1).SendMessage(Arg.Is<string>(x => x.Contains("Build 2 (dev) is still broken by: Joe")));
            chat.ReceivedWithAnyArgs(2).SendMessage(Arg.Any<string>());
            bot.Stop();
        }

        [Test]
        public async Task TeamCityBot_BuildFailedTwoTimesWithStillBrokenDelayBetween_SecondMessageIsPostedImmediately()
        {
            builds.Add(new Build() { Id = "1", Number = "1", Status = "FAILED", StatusText = "Tests failed: 25" });
            var changes = Substitute.For<IChanges>();
            var change = new Change { Username = "Joe" };
            changes.ByLocator(null).ReturnsForAnyArgs(new List<Change> { change });
            tc.Changes.Returns(changes);
            var testOccurrences = Substitute.For<ITestOccurrences>();
            var testOccurrence = new TestOccurrence() { Name = "Assembly: TestName " };
            testOccurrences.ByBuildId(Arg.Any<string>(), Arg.Any<int>()).ReturnsForAnyArgs(new List<TestOccurrence> { testOccurrence });
            tc.TestOccurrences.Returns(testOccurrences);
            bot.StartBot(skype, tc, botParameters, new Dictionary<string, string>(), new TimeConfig { BuildCheckInterval = TimeSpan.FromMilliseconds(10), StillBrokenDelay = TimeSpan.FromMilliseconds(30) });
            Thread.Sleep(50);
            builds.Add(new Build() { Id = "2", Number = "2", Status = "FAILED", StatusText = "Tests failed: 25" });
            Thread.Sleep(50);

            chat.Received(1).SendMessage(Arg.Is<string>(x => x.Contains("Build 1 (dev) is broken by: Joe")));
            chat.Received(1).SendMessage(Arg.Is<string>(x => x.Contains("Build 2 (dev) is still broken by: Joe")));
            chat.ReceivedWithAnyArgs(2).SendMessage(Arg.Any<string>());
            bot.Stop();
        }

        [Test]
        public async Task TeamCityBot_BuildFailedThenFixed_MessageIsPostedImmediately()
        {
            builds.Add(new Build() { Id = "1", Number = "1", Status = "FAILED", StatusText = "Tests failed: 25" });
            builds.Add(new Build() { Id = "2", Number = "2", Status = "SUCCESS" });
            var changes = Substitute.For<IChanges>();
            var change = new Change { Username = "Joe" };
            changes.ByLocator(null).ReturnsForAnyArgs(new List<Change> { change });
            tc.Changes.Returns(changes);
            var testOccurrences = Substitute.For<ITestOccurrences>();
            var testOccurrence = new TestOccurrence() { Name = "Assembly: TestName " };
            testOccurrences.ByBuildId(Arg.Any<string>(), Arg.Any<int>()).ReturnsForAnyArgs(new List<TestOccurrence> { testOccurrence });
            tc.TestOccurrences.Returns(testOccurrences);
            bot.StartBot(skype, tc, botParameters, new Dictionary<string, string>(), new TimeConfig { BuildCheckInterval = TimeSpan.FromMilliseconds(10) });
            Thread.Sleep(3000);

            chat.Received(1).SendMessage(Arg.Is<string>(x => x.Contains("Build 1 (dev) is broken by: Joe")));
            chat.Received(1).SendMessage(Arg.Is<string>(x => x.Contains(" Build 2 (dev) is fixed by: Joe")));
            chat.ReceivedWithAnyArgs(2).SendMessage(Arg.Any<string>());
            bot.Stop();
        }
    }
}

