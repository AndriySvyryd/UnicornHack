using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Actors;

public class PerformActionMessage : IMessage
{
    public const string AIName = "PerformAIAction";
    public const string PlayerName = "PerformPlayerAction";

    public static void EnqueueAI(GameEntity aiEntity, GameManager manager)
    {
        var message = manager.Queue.CreateMessage<PerformActionMessage>(AIName);
        message.Actor = aiEntity;
        manager.Enqueue(message);
    }

    public static void EnqueuePlayer(GameEntity playerEntity, GameManager manager)
    {
        var message = manager.Queue.CreateMessage<PerformActionMessage>(PlayerName);
        message.Actor = playerEntity;
        manager.Enqueue(message);
    }

    private GameEntity _actor;

    public GameEntity Actor
    {
        get => _actor;
        set
        {
            _actor?.RemoveReference(this);
            _actor = value;
            _actor?.AddReference(this);
        }
    }

    string IMessage.MessageName
    {
        get;
        set;
    }

    public void Clean() => Actor = null;
}
