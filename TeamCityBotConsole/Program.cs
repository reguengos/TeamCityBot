using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HelloBotCore;
using SKYPE4COMLib;
using TeamCitySharp;
using TeamCitySharp.DomainEntities;
using TeamCitySharp.Locators;
using Timer = System.Timers.Timer;

namespace TeamCityBot
{
    class Program
    {
        private static Skype skype = new Skype();
        private static HelloBot bot;
        private static Chat publishChat;
        private static string lastCheckedBuildId;
        private static bool wasBroken;
        private static string lastBastard;
        private static object syncRoot = new object();
        private static Random r = new Random();
		private static DateTime? lastFailedTime;

        private static string publishChatName;
        private static string server;
        private static string login;
        private static string password;
        private static string buildConfigId;

        private static List<string> successEmoji = new List<string> { @"\o/", "(^)", "(sun)", "(clap)", "(party)" };
        private static List<string> failEmoji = new List<string> { ";(", "(doh)", "(tmi)", "(facepalm)", "(worry)" };

        static void Main(string[] args)
        {
            if (args.Length < 5)
            {
                Console.WriteLine("Usage: <chat name> <TC server> <login> <password> <build config id>");
                return;
            }

            publishChatName = args[0];
            server = args[1];
            login = args[2];
            password = args[3];
            buildConfigId = args[4];

            bot = new HelloBot();
            bot.OnErrorOccured += BotOnErrorOccured;

            Task.Run(delegate
            {
                try
                {
                    var chats = skype.Chats;

                    foreach (Chat chat in chats)
                    {
                        var t = chat.Topic;
                        if (t == publishChatName)
                        {
                            Console.WriteLine("publish chat found!");
                            publishChat = chat;
                            break;
                        }
                    }

                    skype.MessageStatus += OnMessageReceived;

                    skype.Attach(5, true);
                    Console.WriteLine("skype attached");

                    Timer timer = new Timer();
                    timer.Interval = 15 * 1000;
                    timer.Elapsed += timer_Elapsed;
                    timer.AutoReset = true;
                    timer.Start();

                    while (true)
                    {
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("top lvl exception : " + ex.ToString());
                }
                while (true)
                {
                    Thread.Sleep(1000);
                }
            });

            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        private static string GetRandomEmoji(List<string> emojis)
        {
            var i = r.Next(emojis.Count);
            return emojis[i];
        }

        private static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                lock (syncRoot)
                {
                    var client = new TeamCityClient(server);
                    client.Connect(login, password);

                    var build = client.Builds.LastBuildByBuildConfigId(buildConfigId);

                    if (build != null && build.Id != lastCheckedBuildId)
                    {
                        lastCheckedBuildId = build.Id;
                        Console.WriteLine("Build {0}: {1}", build.Number, build.Status);
						if (build.Status != "SUCCESS" && (!lastFailedTime.HasValue || (DateTime.Now - lastFailedTime.Value).TotalMinutes >= 30))
                        {

                            string msg;
                            if (!wasBroken)
                            {
                                var changes = client.Changes.ByLocator(ChangeLocator.WithBuildId(long.Parse(build.Id))).FirstOrDefault();

                                lastBastard = changes != null ? changes.Username : "<anonymous>";
                                wasBroken = true;

                                msg = String.Format("{0} Build {1} is broken by: {2} {3}", "", build.Number, lastBastard,
                                build.WebUrl);
                            }
                            else
                            {
                                msg = String.Format("{0} Build {1} is still broken by: {2} {3}", "", build.Number, lastBastard,
                                build.WebUrl);
                            }

							lastFailedTime = DateTime.Now;
                            SendMessage(msg, publishChat);
                        }
                        else
                        {
                            if (wasBroken)
                            {
                                var changes = client.Changes.ByLocator(ChangeLocator.WithBuildId(long.Parse(build.Id))).FirstOrDefault();
                                var author = changes != null ? changes.Username : "<anonymous>";
                                var msg = String.Format(@"{0} Build {1} is fixed by: {2}. Nice job!", GetRandomEmoji(successEmoji), build.Number, author);
                                SendMessage(msg, publishChat);
                                wasBroken = false;
                            }
							lastFailedTime = null;
                            wasBroken = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void BotOnErrorOccured(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        private static void OnMessageReceived(ChatMessage pMessage, TChatMessageStatus status)
        {
            Console.WriteLine(status + pMessage.Body);

            if (status == TChatMessageStatus.cmsReceived)
            {
                var cn = pMessage.Chat.Name;
                var topic = pMessage.Chat.Topic;

                bot.HandleMessage(pMessage.Body, answer => SendMessage(answer, pMessage.Chat),
                    new SkypeData() { FromName = pMessage.FromDisplayName });
            }
        }


        public static object _lock = new object();
        private static void SendMessage(string message, Chat toChat)
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
