﻿using System;
using System.Collections.Generic;
using System.Linq;
using TeamCitySharp;
using TeamCitySharp.Locators;

namespace TeamCityBot
{
    public class TeamCityBuildChecker
    {
        private static readonly Random _r = new Random();
        private string _lastBastard;
        private string _lastCheckedBuildId;
        private DateTime? _lastFailedTime;
        private string _lastReason;
        private bool _wasBroken;
        private readonly BuildLocator _buildLocator;
        private readonly ITeamCityClient _client;
        private List<string> _lastFailedTests = new List<string>();
        private readonly string _name;
        private readonly List<string> _successEmoji = new List<string> {@"\o/", "(^)", "(sun)", "(clap)", "(party)"};
        private readonly TimeConfig _timeConfig;

        public TeamCityBuildChecker(BuildLocator buildLocator, ITeamCityClient client, string name, TimeConfig timeConfig)
        {
            _buildLocator = buildLocator;
            _client = client;
            _name = name;
            _timeConfig = timeConfig;
        }

        public void CheckBuild(Action<String> sendMessage)
        {
            var build = _client.Builds.ByBuildLocator(_buildLocator).FirstOrDefault();

            if (build == null)
            {
                Console.WriteLine("No builds found by given locator");
            }
            else if (build.Id == _lastCheckedBuildId)
            {
                Console.WriteLine("Build {0} - already checked last time", build.Id);
            }
            else
            {
                Console.WriteLine("Found new build {0}: {1}", build.Number, build.Status);
                //_lastCheckedBuildId = build.Id;
                if (build.Status != "SUCCESS")
                {
                    var exactBuild = _client.Builds.ByBuildId(build.Id);
                    var reason = exactBuild.StatusText;

                    var now = DateTime.Now;
                    if ((!_lastFailedTime.HasValue ||
                         (now - _lastFailedTime.Value) >= _timeConfig.StillBrokenDelay)
                        ||
                        (String.IsNullOrEmpty(_lastReason) || _lastReason != reason))
                    {
                        _lastCheckedBuildId = build.Id;
                        string msg;
                        if (!_wasBroken)
                        {
                            var changes =
                                _client.Changes.ByLocator(
                                    ChangeLocator.WithBuildId(long.Parse(build.Id)))
                                    .FirstOrDefault();
                            var failReason = GetReason(reason);
                            var detailedReason = "";

                            if (failReason == FailReason.Tests)
                            {
                                var failedTests =
                                    _client.TestOccurrences.ByBuildId(build.Id, 1500)
                                        .Where(x => x.Status != "SUCCESS" && !x.Ignored && !x.Muted)
                                        .Select(x => x.Name.Split(new[] {": "}, StringSplitOptions.None)[1]);

                                var failedMoreCount = failedTests.Count() - 10;
                                var failedShort = failedTests.Take(10);

                                var reasonTail = failedMoreCount > 0
                                    ? Environment.NewLine + "and " + failedMoreCount + " more..."
                                    : "";

                                detailedReason = String.Format("Broken tests: {0}{1}{2}", Environment.NewLine,
                                    String.Join(Environment.NewLine, failedShort), reasonTail);

                                _lastFailedTests = failedTests.ToList();
                            }
                            else
                            {
                                detailedReason = reason; //TODO: get error message from build log
                            }

                            _lastBastard = changes != null ? changes.Username : "<anonymous>";
                            _wasBroken = true;

                            msg = String.Format("{0} Build {1} ({2}) is broken by: {3}{6}{4}{6}{5}{6}{7}", "",
                                build.Number,
                                _name,
                                _lastBastard,
                                reason,
                                build.WebUrl,
                                Environment.NewLine,
                                detailedReason);
                        }
                        else
                        {
                            var failReason = GetReason(reason);
                            var detailedReason = "";

                            if (failReason == FailReason.Tests)
                            {
                                var failedTests =
                                    _client.TestOccurrences.ByBuildId(build.Id, 1500)
                                        .Where(x => x.Status != "SUCCESS" && !x.Ignored && !x.Muted)
                                        .Select(x => x.Name)
                                        .ToList();

                                var newFailedTests = failedTests.Except(_lastFailedTests);
                                var newSuccessTests = _lastFailedTests.Except(failedTests);

                                if (newSuccessTests.Any())
                                {
                                    var failedMoreCount = newFailedTests.Count() - 10;
                                    var failedShort = newFailedTests.Take(10);

                                    var reasonTail = failedMoreCount > 0
                                        ? Environment.NewLine + "and " + failedMoreCount + " more..."
                                        : "";

                                    detailedReason += String.Format("New broken tests: {0}{1}{2}", Environment.NewLine,
                                        String.Join(Environment.NewLine, failedShort), reasonTail);
                                }

                                if (newFailedTests.Any())
                                {
                                    var successMoreCount = newSuccessTests.Count() - 10;
                                    var successShort = newSuccessTests.Take(10);

                                    var reasonTail = successMoreCount > 0
                                        ? Environment.NewLine + "and " + successMoreCount + " more..."
                                        : "";

                                    detailedReason += String.Format("Fixed tests: {0}{1}{2}", Environment.NewLine,
                                        String.Join(Environment.NewLine, successShort), reasonTail);
                                }

                                _lastFailedTests = failedTests;
                            }
                            else
                            {
                                detailedReason = reason;
                            }


                            msg = String.Format("{0} Build {1} ({2}) is still broken by: {3}{6}{4}{6}{5}{6}{7}",
                                "", build.Number, _name, _lastBastard, reason, build.WebUrl,
                                Environment.NewLine, detailedReason);
                        }

                        _lastFailedTime = DateTime.Now;
                        _lastReason = reason;
                        sendMessage(msg);
                    }
                    else
                    {
                        Console.WriteLine(
                            "Build is broken but it's too early to notify: lastFailedTime={0}, now={1}, stillBrokenDelay={2}",
                            _lastFailedTime.Value.ToString("hh:mm:ss.ffff"),
                            now.ToString("hh:mm:ss.ffff"),
                            _timeConfig.StillBrokenDelay.TotalMilliseconds);
                    }
                }
                else
                {
                    if (_wasBroken)
                    {
                        var changes =
                            _client.Changes.ByLocator(
                                ChangeLocator.WithBuildId(long.Parse(build.Id))).FirstOrDefault();
                        var author = changes != null ? changes.Username : "<anonymous>";
                        var msg =
                            String.Format(@"{0} Build {1} ({2}) is fixed by: {3}. Nice job!",
                                GetRandomEmoji(_successEmoji), build.Number, _name, author);
                        sendMessage(msg);
                        _wasBroken = false;
                    }
                    _lastFailedTime = null;
                    _lastReason = null;
                    _wasBroken = false;
                }
            }
        }

        private static string GetRandomEmoji(List<string> emojis)
        {
            var i = _r.Next(emojis.Count);
            return emojis[i];
        }

        private FailReason GetReason(string status)
        {
            if (status.StartsWith("Tests"))
            {
                return FailReason.Tests;
            }
            return FailReason.Build;
        }
    }
}