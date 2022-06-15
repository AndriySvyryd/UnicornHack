using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Senses;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack;

public partial class GameManager
{
    public SensorySystem SensorySystem
    {
        get;
        private set;
    }

    private void InitializeSenses(SequentialMessageQueue<GameManager> queue)
    {
        Add<SensorComponent>(EntityComponent.Sensor, poolSize: 32);

        SensorySystem = new SensorySystem();
        queue.Register<TraveledMessage>(SensorySystem, TraveledMessage.Name, 3);
        queue.Register<EntityAddedMessage<GameEntity>>(SensorySystem, Beings.GetEntityAddedMessageName(), 1);
    }
}
