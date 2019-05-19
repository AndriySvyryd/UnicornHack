using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack
{
    public partial class GameManager
    {
        public EntityGroup<GameEntity> Beings { get; private set; }
        public EntityGroup<GameEntity> Races { get; private set; }
        public SortedEntityRelationship<GameEntity, (int Level, int Id)> RacesToBeingRelationship { get; private set; }
        public LivingSystem LivingSystem { get; private set; }

        private void InitializeBeings(SequentialMessageQueue<GameManager> queue)
        {
            Add<BeingComponent>(EntityComponent.Being, poolSize: 32);
            Add<PhysicalComponent>(EntityComponent.Physical, poolSize: 32);
            Add<RaceComponent>(EntityComponent.Race, poolSize: 32);

            Beings = CreateGroup(nameof(Beings), new EntityMatcher<GameEntity>()
                .AllOf((int)EntityComponent.Being, (int)EntityComponent.Physical, (int)EntityComponent.Sensor));

            Races = CreateGroup(nameof(Races), new EntityMatcher<GameEntity>()
                .AllOf((int)EntityComponent.Race, (int)EntityComponent.Effect));

            RacesToBeingRelationship = new SortedEntityRelationship<GameEntity, (int Level, int Id)>(
                nameof(RacesToBeingRelationship),
                Races,
                Beings,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((EffectComponent)component).AffectedEntityId,
                    (int)EntityComponent.Effect),
                new KeyValueGetter<GameEntity, (int Level, int Id)>(
                    (entity, changes, getOldValue, matcher) =>
                    {
                        if (!matcher.TryGetValue<int>(
                            entity, (int)EntityComponent.Race, nameof(RaceComponent.Level), changes, getOldValue,
                            out var level))
                        {
                            return ((0, 0), false);
                        }

                        return ((level, entity.Id), true);
                    },
                    new PropertyMatcher()
                        .With(component => ((RaceComponent)component).Level, (int)EntityComponent.Race)
                ),
                (effectEntity, _, __) => effectEntity.RemoveComponent((int)EntityComponent.Race));

            LivingSystem = new LivingSystem();
            queue.Add<XPGainedMessage>(LivingSystem, XPGainedMessage.Name, 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
                Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.HitPoints)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
                Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.HitPointMaximum)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
                Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.EnergyPointMaximum)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
                Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.Focus)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
                Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.Might)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
                Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.Speed)), 0);
        }
    }
}
