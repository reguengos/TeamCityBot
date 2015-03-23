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

        public TeamCityBot()
        {

        }

        public Task StartBot(ISkypeAdapter skype, BotParameters botParameters, Dictionary<string, string> moduleParameters)
        {
            _working = true;
            _skype = skype;
            _botParameters = botParameters;
            _bot = new HelloBot(moduleParameters);
            _bot.OnErrorOccured += BotOnErrorOccured;
            
            return Task.Run(delegate
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
                    
                    Timer timer = new Timer { Interval = 15 * 1000 };
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
                    var client = new TeamCityClient(_botParameters.TeamCityServer);
                    client.Connect(_botParameters.TeamCityLogin, _botParameters.TeamCityPassword);

                    var build = client.Builds.LastBuildByBuildConfigId(_botParameters.BuildConfigId);

                    if (build != null && build.Id != _lastCheckedBuildId)
                    {
                        _lastCheckedBuildId = build.Id;
                        Console.WriteLine("Build {0}: {1}", build.Number, build.Status);
                        if (build.Status != "SUCCESS")
                        {
                            if (!_lastFailedTime.HasValue ||
                                (DateTime.Now - _lastFailedTime.Value).TotalMinutes >= 30)
                            {

                                string msg;
                                if (!_wasBroken)
                                {
                                    var changes =
                                        client.Changes.ByLocator(
                                            ChangeLocator.WithBuildId(long.Parse(build.Id)))
                                            .FirstOrDefault();

                                    _lastBastard = changes != null ? changes.Username : "<anonymous>";
                                    _wasBroken = true;

                                    msg = String.Format("{0} Build {1} is broken by: {2} {3}", "",
                                        build.Number, _lastBastard,
                                        build.WebUrl);
                                }
                                else
                                {
                                    msg = String.Format("{0} Build {1} is still broken by: {2} {3}",
                                        "", build.Number, _lastBastard,
                                        build.WebUrl);
                                }

                                _lastFailedTime = DateTime.Now;
                                _publishChat.SendMessage(msg);
                            }
                        }
                        else
                        {
                            if (_wasBroken)
                            {
                                var changes =
                                    client.Changes.ByLocator(
                                        ChangeLocator.WithBuildId(long.Parse(build.Id))).FirstOrDefault();
                                var author = changes != null ? changes.Username : "<anonymous>";
                                var msg =
                                    String.Format(@"{0} Build {1} is fixed by: {2}. Nice job!",
                                        GetRandomEmoji(_successEmoji), build.Number, author);
                                _publishChat.SendMessage(msg);
                                _wasBroken = false;
                            }
                            _lastFailedTime = null;
                            _wasBroken = false;
                        }
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