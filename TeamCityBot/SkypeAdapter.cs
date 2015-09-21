using System;
using System.Security.Cryptography.X509Certificates;
using SKYPE4COMLib;

namespace TeamCityBot
{
    public class SkypeAdapter : ISkypeAdapter
    {
        private readonly Skype _skype;

        public SkypeAdapter(Skype skype)
        {
            skype.Attach();
            skype.MessageStatus += skype_MessageStatus;
	        _skype = skype;
        }

        public event EventHandler<SkypeMessageReceivedEventArgs> OnMessageReceived;

        private void skype_MessageStatus(ChatMessage pMessage, TChatMessageStatus Status)
        {
            if (Status == TChatMessageStatus.cmsReceived)
            {
                var handler = OnMessageReceived;
                if (handler != null)
                {
                    handler(this, new SkypeMessageReceivedEventArgs(pMessage.Body, pMessage.FromDisplayName, new SkypeChat(pMessage.Chat)));
                }
            }
        }

        public IChat GetChat(string topic)
        {
	      foreach (Chat chat in _skype.Chats)
            {
                if (chat.Topic == topic || chat.Name.EndsWith(topic))
                {
                    return new SkypeChat(chat);
                }
            }
            return null;
        }
    }
}