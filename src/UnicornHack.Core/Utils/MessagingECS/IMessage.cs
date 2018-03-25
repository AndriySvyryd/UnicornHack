using System;

namespace UnicornHack.Utils.MessagingECS
{
    public interface IMessage : IDisposable
    {
        string MessageName { get; set; }
    }
}
