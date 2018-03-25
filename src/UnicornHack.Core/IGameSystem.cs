using UnicornHack.Utils.MessagingECS;

namespace UnicornHack
{
    public interface IGameSystem<TMessage> : IMessageConsumer<TMessage, GameManager>
    {
    }
}
