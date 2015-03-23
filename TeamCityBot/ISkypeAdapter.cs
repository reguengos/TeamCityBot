using System;

namespace TeamCityBot
{
    public interface ISkypeAdapter
    {
        IChat GetChat(string topic);
        event EventHandler<SkypeMessageReceivedEventArgs> OnMessageReceived;
    }
}