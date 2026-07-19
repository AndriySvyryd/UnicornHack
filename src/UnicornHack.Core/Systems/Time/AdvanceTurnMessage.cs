using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Time;

public class AdvanceTurnMessage : IMessage
{
    public const string Name = "AdvanceTurn";

    public static void Enqueue(GameManager manager)
        => manager.Enqueue(manager.Queue.CreateMessage<AdvanceTurnMessage>(Name), lowPriority: true);

    string IMessage.MessageName
    {
        get => Name;
        set => Debug.Assert(value == Name);
    }

    public void Clean()
    {
    }
}
