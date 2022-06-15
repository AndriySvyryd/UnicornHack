using System.Collections.Generic;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
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
    }

    public EntityGroup<GameEntity> Races
    {
        get;
        private set;
    }

    public CollectionEntityRelationship<GameEntity, HashSet<GameEntity>> RacesToBeingRelationship
    {
        get;
        private set;
    }

    public LivingSystem LivingSystem
    {
        get;
        private set;
    }

    private void InitializeBeings(SequentialMessageQueue<GameManager> queue)
    {
        Add<BeingComponent>(EntityComponent.Being, poolSize: 32);
        Add<PhysicalComponent>(EntityComponent.Physical, poolSize: 32);
        Add<RaceComponent>(EntityComponent.Race, poolSize: 32);

        Beings = CreateGroup(nameof(Beings), new EntityMatcher<GameEntity>()
            .AllOf((int)EntityComponent.Being, (int)EntityComponent.Physical, (int)EntityComponent.Sensor));

        Races = CreateGroup(nameof(Races), new EntityMatcher<GameEntity>()
            .AllOf((int)EntityComponent.Race, (int)EntityComponent.Effect));

        RacesToBeingRelationship = new(
            nameof(RacesToBeingRelationship),
            Races,
            Beings,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((EffectComponent)component).AffectedEntityId,
                (int)EntityComponent.Effect),
            (raceEntity, _) => raceEntity.RemoveComponent((int)EntityComponent.Race),
            beingEntity => (HashSet<GameEntity>)beingEntity.Being.Races);

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
