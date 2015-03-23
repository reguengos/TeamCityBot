namespace TeamCityBot
{
    public class SkypeMessageReceivedEventArgs
    {
        public SkypeMessageReceivedEventArgs(string body, string fromDisplayName, IChat chat)
        {
            Body = body;
            Chat = chat;
            FromDisplayName = fromDisplayName;
        }

        public string Body { get; private set; }
        public string FromDisplayName { get; private set; }
        public IChat Chat { get; private set; }
    }
}