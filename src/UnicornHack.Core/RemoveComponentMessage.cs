using UnicornHack.Utils.MessagingECS;

namespace UnicornHack
{
    public class RemoveComponentMessage : IMessage
    {
        public GameEntity Entity { get; set; }
        public EntityComponent Component { get; set; }

        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
        }
    }
}
