using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCitySharp.DomainEntities;

namespace TeamCityBot
{
    public class TestOccurrenceComparerByName : IEqualityComparer<TestOccurrence>
    {
        public bool Equals(TestOccurrence x, TestOccurrence y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(TestOccurrence obj)
        {
            return obj.Name[0];
        }
    }
}
