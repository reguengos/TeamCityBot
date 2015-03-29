using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCityBot
{
    public static class DateTimeProvider
    {
        private static Func<DateTime> _nowFunc;
        public static DateTime UtcNow { get { return _nowFunc(); }  }

        static DateTimeProvider()
        {
            _nowFunc = () => DateTime.UtcNow;
        }

        public static void SetFunc(Func<DateTime> nowFunc)
        {
            _nowFunc = nowFunc;
        }

        public static void SetValue(DateTime now)
        {
            _nowFunc = () => now;
        }
    }
}
