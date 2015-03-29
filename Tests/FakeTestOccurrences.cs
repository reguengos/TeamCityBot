using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCitySharp.ActionTypes;
using TeamCitySharp.DomainEntities;

namespace Tests
{
    class FakeTestOccurrences : ITestOccurrences
    {
        Dictionary<string, List<TestOccurrence>> _dict;

        public FakeTestOccurrences()
        {
            _dict = new Dictionary<string, List<TestOccurrence>>();
        }

        public List<TeamCitySharp.DomainEntities.TestOccurrence> ByBuildId(string buildId, int count)
        {
            return _dict.ContainsKey(buildId) ? _dict[buildId] : new List<TestOccurrence>();
        }

        public void Add(string buildId, params TestOccurrence[] occurrences)
        {
            _dict[buildId] = occurrences.ToList();
        }
    }
}
