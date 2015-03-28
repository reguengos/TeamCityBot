using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using HelloBotCore;
using TeamCitySharp;
using TeamCitySharp.Locators;
using Timer = System.Timers.Timer;

namespace TeamCityBot
{
    public class TeamCityBot
    {
        private static List<TeamCityBuildChecker> buildCheckers;
        private HelloBot _bot;
        private BotParameters _botParameters;
        private string _lastBastard;
        private string _lastCheckedBuildId;
        private DateTime? _lastFailedTime;
        private IChat _publishChat;
        private ISkypeAdapter _skype;
        private ITeamCityClient _teamcity;
        private Timer _timer;
        private bool _wasBroken;
        private bool _working;
        private readonly object _lock = new object();
        private readonly Random _r = new Random();
        private readonly List<string> _successEmoji = new List<string> {@"\o/", "(^)", "(sun)", "(clap)", "(party)"};
        private readonly object _syncRoot = new object();
        private TimeConfig _timeConfig;

        public Task StartBot(ISkypeAdapter skype, ITeamCityClient teamCity, BotParameters botParameters,
            Dictionary<string, string> moduleParameters, TimeConfig timeConfig)
        {
            _working = true;
            _skype = skype;
            _teamcity = teamCity;
            _botParameters = botParameters;
            _timeConfig = timeConfig;
            _bot = new HelloBot(moduleParameters);
            _bot.OnErrorOccured += BotOnErrorOccured;

            var task = Task.Run(delegate
            {
                try
                {
                    _publishChat = _skype.GetChat(botParameters.PublishChatName);
                    if (_publishChat != null)
                    {
                        Console.WriteLine("publish chat found!");
                    }
                    else
                    {
                        Console.WriteLine("publish chat NOT found!");
                    }
                    _skype.OnMessageReceived += OnMessageReceived;

                    _timer = new Timer {Interval = timeConfig.BuildCheckInterval.TotalMilliseconds};
                    _timer.Elapsed += timer_Elapsed;
                    _timer.AutoReset = false;
                    _timer.Start();

                    while (_working)
                    {
                        Thread.Sleep(5);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("top lvl exception : " + ex);
                }
            });

            buildCheckers =
                _botParameters.Branches.Select(
                    x =>
                        new TeamCityBuildChecker(
                            BuildLocator.WithDimensions(BuildTypeLocator.WithId(_botParameters.BuildConfigId),
                                branch: x), _teamcity, x, _timeConfig)).ToList();

            return task;
        }

        public void Stop()
        {
            _working = false;
            _timer.Stop();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var r = Guid.NewGuid();
                lock (_syncRoot)
                {
                    //Console.WriteLine("enter " + r);
                    foreach (var checker in buildCheckers)
                    {
                        checker.CheckBuild(msg => SendMessage(msg, _publishChat));
                    }
                    //Console.WriteLine("exit " + r);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            _timer.Start();
        }

        private static void BotOnErrorOccured(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        private void OnMessageReceived(object sender, SkypeMessageReceivedEventArgs e)
        {
            Console.WriteLine("{0}: {1}", e.FromDisplayName, e.Body);

            _bot.HandleMessage(e.Body, answer => SendMessage(answer, e.Chat),
                new SkypeData {FromName = e.FromDisplayName});
        }

        private void SendMessage(string message, IChat toChat)
        {
            if (toChat != null)
            {
                if (message.StartsWith("/"))
                {
                    message = "(heidy) " + message;
                }
                Console.WriteLine("SENDMESSAGE:" + message);
                toChat.SendMessage(message);
            }
            else
            {
                Console.WriteLine("publish chat is null!");
            }
        }
    }
}