using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Senses;

public class SoundMessage : IMessage
{
    public const string Name = "Sound";

    private GameEntity? _source;

    public GameEntity Source
    {
        get => _source!;
        set
        {
            _source?.RemoveReference(this);
            _source = value;
            _source?.AddReference(this);
        }
    }

    public int ActionNoise { get; set; }

    public static void Enqueue(GameEntity source, int actionNoise, GameManager manager)
    {
        var message = manager.Queue.CreateMessage<SoundMessage>(Name);
        message.Source = source;
        message.ActionNoise = actionNoise;
        manager.Enqueue(message);
    }

    string IMessage.MessageName { get; set; } = null!;

    public void Clean()
    {
        Source = null!;
        ActionNoise = 0;
    }
}