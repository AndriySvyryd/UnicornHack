namespace UnicornHack.Utils.MessagingECS
{
    public interface IMessageQueue
    {
        TMessage CreateMessage<TMessage>(string name)
            where TMessage : class, IMessage, new();

        void ReturnMessage<TMessage>(TMessage message)
            where TMessage : class, IMessage, new();

        void Enqueue<TMessage>(TMessage message, bool lowPriority = false)
            where TMessage : class, IMessage, new();
    }
}
