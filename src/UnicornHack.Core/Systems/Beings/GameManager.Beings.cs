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
        public SortedEntityRelationship<GameEntity, (byte Level, int Id)> RacesToBeingRelationship { get; private set; }
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

            RacesToBeingRelationship = new SortedEntityRelationship<GameEntity, (byte Level, int Id)>(
                nameof(RacesToBeingRelationship),
                Races,
                Beings,
                new SimpleNonNullableKeyValueGetter<GameEntity, int>(
                    component => ((EffectComponent)component).AffectedEntityId,
                    (int)EntityComponent.Effect),
                new KeyValueGetter<GameEntity, (byte Level, int Id)>(
                    (entity, changedComponentId, changedComponent, changedProperty, changedValue) =>
                    {
                        if (changedComponentId == (int)EntityComponent.Race)
                        {
                            if (changedProperty != null)
                            {
                                return (((byte)changedValue, entity.Id), true);
                            }
                        }
                        else
                        {
                            changedComponent = entity.Race;
                        }

                        return ((((RaceComponent)changedComponent).Level, entity.Id), true);
                    },
                    new PropertyMatcher((int)EntityComponent.Race, nameof(RaceComponent.Level))
                ),
                (effectEntity, _, __, ___) => effectEntity.RemoveComponent((int)EntityComponent.Race));

            LivingSystem = new LivingSystem();
            queue.Add<XPGainedMessage>(LivingSystem, KnowledgeSystem.XPGainedMessageName, 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
                Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.HitPointMaximum)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
                Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.EnergyPointMaximum)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
                Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.Willpower)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
                Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.Constitution)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(LivingSystem,
                Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.Quickness)), 0);
        }
    }
}
