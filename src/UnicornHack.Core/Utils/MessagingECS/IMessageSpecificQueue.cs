namespace UnicornHack.Utils.MessagingECS;

// ReSharper disable once TypeParameterCanBeVariant
public interface IMessageSpecificQueue<TState>
{
    void ProcessNext(TState state, IMessageQueue queue);
}
