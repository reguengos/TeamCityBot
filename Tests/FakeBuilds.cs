using System;
using System.Collections.Generic;
using System.Linq;
using TeamCitySharp.ActionTypes;
using TeamCitySharp.DomainEntities;
using TeamCitySharp.Locators;

namespace Tests
{
    public class FakeBuilds : IBuilds
    {
        private List<Build> builds = new List<Build>();
        private int counter = -1;
        private object _lock = new object();

        public void Add(Build build)
        {
            builds.Add(build);
        }


        public void Add2QueueBuildByBuildConfigId(string buildConfigId)
        {
            throw new NotImplementedException();
        }

        public List<Build> AllBuildsOfStatusSinceDate(DateTime date, BuildStatus buildStatus)
        {
            throw new NotImplementedException();
        }

        public List<Build> AllSinceDate(DateTime date)
        {
            throw new NotImplementedException();
        }

        public List<Build> ByBranch(string branchName)
        {
            throw new NotImplementedException();
        }

        public List<Build> ByBuildConfigId(string buildConfigId)
        {
            throw new NotImplementedException();
        }

        public Build ByBuildId(string id)
        {
            return builds.FirstOrDefault(x => x.Id == id);
        }

        public List<Build> ByBuildLocator(BuildLocator locator)
        {
            lock (_lock)
            {
                var result = new List<Build>();

                if (builds.Count > counter + 1)
                {
                    counter++;
                }

                if (builds.Count >= counter + 1)
                {
                    result.Add(builds[counter]);
                }

                return result;
            }
        }

        public List<Build> ByConfigIdAndTag(string buildConfigId, string tag)
        {
            throw new NotImplementedException();
        }

        public List<Build> ByUserName(string userName)
        {
            throw new NotImplementedException();
        }

        public List<Build> ErrorBuildsByBuildConfigId(string buildConfigId)
        {
            throw new NotImplementedException();
        }

        public List<Build> FailedBuildsByBuildConfigId(string buildConfigId)
        {
            throw new NotImplementedException();
        }

        public Build LastBuildByAgent(string agentName)
        {
            throw new NotImplementedException();
        }

        public Build LastBuildByBuildConfigId(string buildConfigId)
        {
            throw new NotImplementedException();
        }

        public Build LastErrorBuildByBuildConfigId(string buildConfigId)
        {
            throw new NotImplementedException();
        }

        public Build LastFailedBuildByBuildConfigId(string buildConfigId)
        {
            throw new NotImplementedException();
        }

        public Build LastSuccessfulBuildByBuildConfigId(string buildConfigId)
        {
            throw new NotImplementedException();
        }

        public List<Build> NonSuccessfulBuildsForUser(string userName)
        {
            throw new NotImplementedException();
        }

        public List<Build> SuccessfulBuildsByBuildConfigId(string buildConfigId)
        {
            throw new NotImplementedException();
        }
    }
}