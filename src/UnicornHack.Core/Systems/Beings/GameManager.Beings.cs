using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack;

public partial class GameManager
{
    public EntityGroup<GameEntity> Beings
    {
        get;
        private set;
    } = null!;

    public LivingSystem LivingSystem
    {
        get;
        private set;
    } = null!;

    private void InitializeBeings(SequentialMessageQueue<GameManager> queue)
    {
        Add<BeingComponent>(EntityComponent.Being, poolSize: 512, maxPoolSize: 1024);
        Add<PhysicalComponent>(EntityComponent.Physical, poolSize: 512, maxPoolSize: 1024);

        Beings = CreateGroup(nameof(Beings), new EntityMatcher<GameEntity>()
            .AllOf((int)EntityComponent.Being, (int)EntityComponent.Physical), capacity: 512);

        LivingSystem = new LivingSystem();
        queue.Register<XPGainedMessage>(LivingSystem, XPGainedMessage.Name, 0);
        queue.Register<EntityAddedMessage<GameEntity>>(LivingSystem, Beings.GetEntityAddedMessageName(), 0);
        queue.Register<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
            Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.HitPoints)), 0);
        queue.Register<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
            Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.HitPointMaximum)), 0);
        queue.Register<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
            Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.EnergyPointMaximum)), 0);
        queue.Register<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
            Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.ReservedHitPoints)), 0);
        queue.Register<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
            Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.ReservedEnergyPoints)), 0);
        queue.Register<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
            Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.Focus)), 0);
        queue.Register<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
            Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.Might)), 0);
        queue.Register<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
            Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.Speed)), 0);
        queue.Register<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
            Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.Perception)), 0);
        queue.Register<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
            Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.Hindrance)), 0);
    }
}
