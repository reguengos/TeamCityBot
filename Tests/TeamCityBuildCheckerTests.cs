using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using TeamCityBot;
using TeamCitySharp;
using TeamCitySharp.ActionTypes;
using TeamCitySharp.DomainEntities;
using TeamCitySharp.Locators;

namespace Tests
{
    public class TeamCityBuildCheckerTests
    {
        private ITeamCityClient tc;
        private BotParameters botParameters;
        private FakeBuilds builds;
        private FakeTestOccurrences testOccurrences;
        private List<string> _sendMessages;
        private Action<string> _sendMessageAction;
        
        [SetUp]
        public void SetUp()
        {
            Console.WriteLine("Setup start");
            tc = Substitute.For<ITeamCityClient>();
            builds = new FakeBuilds();
            tc.Builds.Returns(builds);

            testOccurrences = new FakeTestOccurrences();
            tc.TestOccurrences.Returns(testOccurrences);

            _sendMessages = new List<string>();
            _sendMessageAction = msg => _sendMessages.Add(msg);

            Console.WriteLine("Setup finish");
        }

        public void TeamCityBuildChecker_BuildFailed_MessageIsPosted()
        {
            var checker = new TeamCityBuildChecker(new BuildLocator(), tc, "dev", new TimeConfig { StillBrokenDelay = TimeSpan.FromMilliseconds(10) });
            builds.Add(new Build() { Id = "1", Number = "1", Status = "FAILED", StatusText = "Tests failed: 25" });
            var changes = Substitute.For<IChanges>();
            var change = new Change { Username = "Joe" };
            changes.ByLocator(null).ReturnsForAnyArgs(new List<Change> { change });
            tc.Changes.Returns(changes);
            
            checker.CheckBuild(_sendMessageAction);
            Assert.AreEqual(1, _sendMessages.Count);
            Assert.True(_sendMessages[0].Contains("is broken"));
        }

        [Test]
        public void TeamCityBuildChecker_BuildFailedTwoTimesInARow_SecondMessageIsPostedAfterStillBrokenDelay()
        {
            var checker = new TeamCityBuildChecker(new BuildLocator(), tc, "dev", new TimeConfig { StillBrokenDelay = TimeSpan.FromMilliseconds(10)});
            builds.Add(new Build() { Id = "1", Number = "1", Status = "FAILED", StatusText = "Tests failed: 25" });
            builds.Add(new Build() { Id = "2", Number = "2", Status = "FAILED", StatusText = "Tests failed: 25" });
            var changes = Substitute.For<IChanges>();
            var change = new Change { Username = "Joe" };
            changes.ByLocator(null).ReturnsForAnyArgs(new List<Change> { change });
            tc.Changes.Returns(changes);
            var now = new DateTime();
            DateTimeProvider.SetFunc(() => now);

            checker.CheckBuild(_sendMessageAction);
            Assert.AreEqual(1, _sendMessages.Count);
            Assert.True(_sendMessages[0].Contains("is broken"));
            now = now.AddMilliseconds(7);
            checker.CheckBuild(_sendMessageAction);
            Assert.AreEqual(1, _sendMessages.Count);
            now = now.AddMilliseconds(7);
            checker.CheckBuild(_sendMessageAction);
            Assert.AreEqual(2, _sendMessages.Count);
            Assert.True(_sendMessages[1].Contains("still broken"));
        }

        [Test]
        public void TeamCityBuildChecker_BuildFailedSeveralTimesDuringStillBrokenDelay_PostedOnlyLastMessage()
        {
            var checker = new TeamCityBuildChecker(new BuildLocator(), tc, "dev", new TimeConfig { StillBrokenDelay = TimeSpan.FromMilliseconds(10) });
            builds.Add(new Build() { Id = "1", Number = "1", Status = "FAILED", StatusText = "Tests failed: 25" });
            builds.Add(new Build() { Id = "2", Number = "2", Status = "FAILED", StatusText = "Tests failed: 25" });
            builds.Add(new Build() { Id = "3", Number = "3", Status = "FAILED", StatusText = "Tests failed: 25" });
            builds.Add(new Build() { Id = "4", Number = "4", Status = "FAILED", StatusText = "Tests failed: 25" });
            var changes = Substitute.For<IChanges>();
            var change = new Change { Username = "Joe" };
            changes.ByLocator(null).ReturnsForAnyArgs(new List<Change> { change });
            tc.Changes.Returns(changes);
            var now = new DateTime();
            DateTimeProvider.SetFunc(() => now);

            checker.CheckBuild(_sendMessageAction);
            Assert.AreEqual(1, _sendMessages.Count);
            Assert.True(_sendMessages[0].Contains("is broken"));
            now = now.AddMilliseconds(3);
            checker.CheckBuild(_sendMessageAction);
            Assert.AreEqual(1, _sendMessages.Count);
            now = now.AddMilliseconds(3);
            checker.CheckBuild(_sendMessageAction);
            Assert.AreEqual(1, _sendMessages.Count);
            now = now.AddMilliseconds(3);
            checker.CheckBuild(_sendMessageAction);
            Assert.AreEqual(1, _sendMessages.Count);
            now = now.AddMilliseconds(3);
            checker.CheckBuild(_sendMessageAction);
            Assert.AreEqual(2, _sendMessages.Count);
            Assert.True(_sendMessages[1].Contains("still broken"));
            Assert.True(_sendMessages[1].Contains("Build 4"));
        }

        [Test]
        public void TeamCityBuildChecker_BuildFailedTwoTimesInARowWithStillBrokenDelayInBetween_SecondMessageIsPostedImmediately()
        {
            var checker = new TeamCityBuildChecker(new BuildLocator(), tc, "dev", new TimeConfig { StillBrokenDelay = TimeSpan.FromMilliseconds(10) });
            builds.Add(new Build() { Id = "1", Number = "1", Status = "FAILED", StatusText = "Tests failed: 25" });
            builds.Add(new Build() { Id = "2", Number = "2", Status = "FAILED", StatusText = "Tests failed: 25" });
            var changes = Substitute.For<IChanges>();
            var change = new Change { Username = "Joe" };
            changes.ByLocator(null).ReturnsForAnyArgs(new List<Change> { change });
            tc.Changes.Returns(changes);
            var now = new DateTime();
            DateTimeProvider.SetFunc(() => now);

            checker.CheckBuild(_sendMessageAction);
            Assert.AreEqual(1, _sendMessages.Count);
            Assert.True(_sendMessages[0].Contains("is broken"));
            now = now.AddMilliseconds(11);
            checker.CheckBuild(_sendMessageAction);
            Assert.AreEqual(2, _sendMessages.Count);
            Assert.True(_sendMessages[1].Contains("still broken"));
        }

        [Test]
        public void TeamCityBot_BuildFailedThenFixed_MessageIsPostedImmediately()
        {
            builds.Add(new Build() { Id = "1", Number = "1", Status = "FAILED", StatusText = "Tests failed: 25" });
            builds.Add(new Build() { Id = "2", Number = "2", Status = "SUCCESS" });
            var changes = Substitute.For<IChanges>();
            var change = new Change { Username = "Joe" };
            changes.ByLocator(null).ReturnsForAnyArgs(new List<Change> { change });
            tc.Changes.Returns(changes);
            var checker = new TeamCityBuildChecker(new BuildLocator(), tc, "dev", new TimeConfig { StillBrokenDelay = TimeSpan.FromMilliseconds(10) });

            checker.CheckBuild(_sendMessageAction);
            Assert.AreEqual(1, _sendMessages.Count);
            Assert.True(_sendMessages[0].Contains("is broken"));
            checker.CheckBuild(_sendMessageAction);
            Assert.AreEqual(2, _sendMessages.Count);
            Assert.True(_sendMessages[1].Contains("fixed"));
        }
    }
}
