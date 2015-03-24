﻿using System;
using System.Collections.Generic;
using System.Linq;
using SKYPE4COMLib;
using TeamCitySharp;
using TeamCitySharp.Locators;

namespace TeamCityBot
{
    class TeamCityBuildChecker
    {
        private string _lastCheckedBuildId;
        private bool _wasBroken;
        private string _lastBastard;
        private DateTime? _lastFailedTime;
        private string _lastReason;
        private List<string> _successEmoji = new List<string> { @"\o/", "(^)", "(sun)", "(clap)", "(party)" };
        private static Random _r = new Random();
        private string _name;
        private BuildLocator _buildLocator;
        private ITeamCityClient _client;

        public TeamCityBuildChecker(BuildLocator buildLocator, ITeamCityClient client, string name)
        {
            this._buildLocator = buildLocator;
            this._client = client;
            this._name = name;
        }

        public void CheckBuild(Action<String> sendMessage)
        {
            var build = _client.Builds.ByBuildLocator(_buildLocator).FirstOrDefault();

            if (build != null && build.Id != _lastCheckedBuildId)
            {
                _lastCheckedBuildId = build.Id;
                Console.WriteLine("Build {0}: {1}", build.Number, build.Status);
                if (build.Status != "SUCCESS")
                {
                    var exactBuild = _client.Builds.ByBuildId(build.Id);
                    var reason = exactBuild.StatusText;

                    if ((!_lastFailedTime.HasValue || (DateTime.Now - _lastFailedTime.Value).TotalMinutes >= 60)
                        ||
                        (String.IsNullOrEmpty(_lastReason) || _lastReason != reason))
                    {

                        string msg;
                        if (!_wasBroken)
                        {
                            var changes =
                                _client.Changes.ByLocator(
                                    ChangeLocator.WithBuildId(long.Parse(build.Id)))
                                    .FirstOrDefault();

                            _lastBastard = changes != null ? changes.Username : "<anonymous>";
                            _wasBroken = true;

                            msg = String.Format("{0} Build {1} ({2}) is broken by: {3}{6}{4}{6}{5}", "",
                                build.Number,
                                _name,
                                _lastBastard,
                                reason,
                                build.WebUrl,
                                Environment.NewLine);
                        }
                        else
                        {
                            msg = String.Format("{0} Build {1} ({2}) is still broken by: {3}{6}{4}{6}{5}",
                                "", build.Number, _name, _lastBastard, reason, build.WebUrl,
                                Environment.NewLine);


                        }

                        _lastFailedTime = DateTime.Now;
                        _lastReason = reason;
                        sendMessage(msg);
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
    }
}