using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCityBot
{
    public class BuildResult
    {
        private string author;
        private BuildStatus status;
        private FailReason reason;
        private string detailedReason;
        private string reasonText;
        private string webUrl;

        public string WebUrl
        {
            get { return webUrl; }
            set { webUrl = value; }
        }

        public string ReasonText
        {
            get { return reasonText; }
            set { reasonText = value; }
        }

        public string DetailedReason
        {
            get { return detailedReason; }
            set { detailedReason = value; }
        }

        private string number;
        private string branch;

        public string Branch
        {
            get { return branch; }
            set { branch = value; }
        }

        private List<string> brokenTests;
        private List<string> fixedTests;
        private List<string> newBrokenTests;

        public string Author
        {
            get { return author; }
            set { author = value; }
        }

        public BuildStatus Status
        {
            get { return status; }
            set { status = value; }
        }

        public FailReason Reason
        {
            get { return reason; }
            set { reason = value; }
        }

        public string Number
        {
            get { return number; }
            set { number = value; }
        }

        public List<string> BrokenTests
        {
            get { return brokenTests; }
            set { brokenTests = value; }
        }

        public List<string> FixedTests
        {
            get { return fixedTests; }
            set { fixedTests = value; }
        }

        public List<string> NewBrokenTests
        {
            get { return newBrokenTests; }
            set { newBrokenTests = value; }
        }
    }
}
