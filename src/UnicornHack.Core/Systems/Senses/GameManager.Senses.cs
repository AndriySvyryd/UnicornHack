using UnicornHack.Systems.Beings;
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
    } = null!;

    private void InitializeSenses(SequentialMessageQueue<GameEntity> queue)
    {
        SensorySystem = new SensorySystem();
        queue.Register<TraveledMessage>(SensorySystem, TraveledMessage.Name, 3);
        queue.Register<EntityAddedMessage<GameEntity>>(SensorySystem, Beings.GetEntityAddedMessageName(), 1);
        queue.Register<PropertyValueChangedMessage<GameEntity, int>>(
            SensorySystem,
            Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.PrimaryVisionRange)), 0);
        queue.Register<PropertyValueChangedMessage<GameEntity, int>>(
            SensorySystem,
            Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.SecondaryVisionRange)), 0);
    }
}
