using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HelloBotCore;
using TeamCitySharp;
using TeamCitySharp.Locators;
using Timer = System.Timers.Timer;

namespace TeamCityBot
{
    public class TeamCityBot
    {
        private ISkypeAdapter _skype;
        private ITeamCityClient _teamcity;
        private HelloBot _bot;
        private IChat _publishChat;
        private string _lastCheckedBuildId;
        private bool _wasBroken;
        private bool _working;
        private string _lastBastard;
        private readonly object _syncRoot = new object();
        private readonly Random _r = new Random();
        private DateTime? _lastFailedTime;
        private readonly List<string> _successEmoji = new List<string> { @"\o/", "(^)", "(sun)", "(clap)", "(party)" };
        private BotParameters _botParameters;
        private readonly object _lock = new object();
        private static List<TeamCityBuildChecker> buildCheckers;

        public TeamCityBot()
        {

        }

        public Task StartBot(ISkypeAdapter skype, ITeamCityClient teamCity, BotParameters botParameters, Dictionary<string, string> moduleParameters, double interval = 15000)
        {
            _working = true;
            _skype = skype;
            _teamcity = teamCity;
            _botParameters = botParameters;
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
                    
                    Timer timer = new Timer { Interval = interval };
                    timer.Elapsed += timer_Elapsed;
                    timer.AutoReset = true;
                    timer.Start();

                    while (_working)
                    {
                        Thread.Sleep(1000);
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
                            BuildLocator.WithDimensions(buildType: BuildTypeLocator.WithId(_botParameters.BuildConfigId),
                                branch: x), _teamcity, x)).ToList();

            return task;
        }

        public void Stop()
        {
            _working = false;
        }

        private string GetRandomEmoji(List<string> emojis)
        {
            var i = _r.Next(emojis.Count);
            return emojis[i];
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                lock (_syncRoot)
                {
                    foreach (var checker in buildCheckers)
                    {
                        checker.CheckBuild(msg => SendMessage(msg, _publishChat));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void BotOnErrorOccured(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        private void OnMessageReceived(object sender, SkypeMessageReceivedEventArgs e)
        {
            Console.WriteLine("{0}: {1}", e.FromDisplayName, e.Body);

            _bot.HandleMessage(e.Body, answer => SendMessage(answer, e.Chat),
                    new SkypeData() { FromName = e.FromDisplayName });
        }

        private void SendMessage(string message, IChat toChat)
        {
            if (toChat != null)
            {
                if (message.StartsWith("/"))
                {
                    message = "(heidy) " + message;
                }
                lock (_lock)
                {
                    toChat.SendMessage(message);
                }
            }
        }
    }
}