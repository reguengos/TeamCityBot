using System;
using System.Collections.Generic;
using System.Linq;
using TeamCitySharp.DomainEntities;

namespace TeamCityBot
{
    public class TestOccurrencesCollection
    {
        private readonly IEnumerable<TestOccurrence> _occurrences;

        public IEnumerable<TestOccurrence> Occurrences
        {
            get { return _occurrences; }
        }

        private Dictionary<string, TestOccurrence> _occurencesDict = new Dictionary<string, TestOccurrence>();

        public TestOccurrencesCollection(IEnumerable<TestOccurrence> occurrences)
        {
            _occurrences = occurrences;
            _occurencesDict = occurrences.ToDictionary(x => x.Name, x => x);
        }

        public IEnumerable<TestOccurrence> Ignored
        {
            get { return _occurrences.Where(x => x.Ignored); }
        }

        public int IgnoredCount
        {
            get { return Ignored.Count(); }
        }

        public IEnumerable<TestOccurrence> Failed
        {
            get { return _occurrences.Where(x => x.Status != "SUCCESS" && !x.Ignored && !x.Muted); }
        }

        public int SuccessCount
        {
            get { return Success.Count(); }
        }

        public IEnumerable<TestOccurrence> Success
        {
            get { return _occurrences.Where(x => x.Status == "SUCCESS"); }
        }

        public int FailedCount
        {
            get { return Failed.Count(); }
        }

        public TestOccurrence GetByName(string name)
        {
            return _occurencesDict.ContainsKey(name) ? _occurencesDict[name] : null;
        }

        public string Show(bool showFailed=true, string failedPrefix="Broken tests", bool showSuccess=false, string successPrefix="Passed tests")
        {
            string result = "";
            if (showFailed)
            {
                var failedTests = Failed.Select(x => x.Name.Split(new[] {": "}, StringSplitOptions.None)[1]).ToList();

                if (failedTests.Any())
                {

                    var failedMoreCount = failedTests.Count() - 10;
                    var failedShort = failedTests.Take(10);

                    var reasonTail = failedMoreCount > 0
                        ? Environment.NewLine + "and " + failedMoreCount + " more..."
                        : "";

                    result += String.Format("{3}: {0}{1}{2}", Environment.NewLine,
                        String.Join(Environment.NewLine, failedShort), reasonTail, failedPrefix);
                }
            }

            if (showSuccess)
            {
                var passedTests = Success.Select(x => x.Name.Split(new[] { ": " }, StringSplitOptions.None)[1]).ToList();

                if (passedTests.Any())
                {

                    var passedMoreCount = passedTests.Count() - 10;
                    var passedShort = passedTests.Take(10);

                    var reasonTail = passedMoreCount > 0
                        ? Environment.NewLine + "and " + passedMoreCount + " more..."
                        : "";

                    result += String.Format("{3}: {0}{1}{2}", Environment.NewLine,
                        String.Join(Environment.NewLine, passedShort), reasonTail, successPrefix);
                }
            }

            return result;
        }

        public TestOccurrencesCollection Diff(TestOccurrencesCollection other)
        {
            var failed = other.Failed.Except(this.Failed, new TestOccurrenceComparerByName());
            var success = other.Success.Except(this.Success, new TestOccurrenceComparerByName());

            return new TestOccurrencesCollection(failed.Union(success));
        }

        public bool EqualsByFailed(TestOccurrencesCollection other)
        {
            return this.Failed.Select(x => x.Name).SequenceEqual(other.Failed.Select(x => x.Name));
        }
    }
}