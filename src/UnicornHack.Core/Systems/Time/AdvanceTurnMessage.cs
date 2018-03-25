using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Time
{
    public class AdvanceTurnMessage : IMessage
    {
        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
        }
    }
}
