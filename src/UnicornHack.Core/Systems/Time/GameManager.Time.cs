using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Time;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack
{
    public partial class GameManager
    {
        public EntityGroup<GameEntity> TemporalEntities { get; private set; }
        public SortedUniqueEntityIndex<GameEntity, (int Tick, int Id)> TemporalEntitiesIndex { get; private set; }
        public TimeSystem TimeSystem { get; private set; }

        private void InitializeTime(SequentialMessageQueue<GameManager> queue)
        {
            TemporalEntities = CreateGroup(nameof(TemporalEntities),
                new EntityMatcher<GameEntity>().AnyOf(
                    (int)EntityComponent.AI,
                    (int)EntityComponent.Player,
                    (int)EntityComponent.Effect));

            TemporalEntitiesIndex = new SortedUniqueEntityIndex<GameEntity, (int Tick, int Id)>(
                TemporalEntities,
                new KeyValueGetter<GameEntity, (int Tick, int Id)>(
                    (entity, changedComponentId, changedComponent, changedProperty, changedValue) =>
                    {
                        int? tick;
                        switch (changedComponentId)
                        {
                            case (int)EntityComponent.AI:
                                if (changedProperty == nameof(AIComponent.NextActionTick))
                                {
                                    tick = (int?)changedValue;
                                }
                                else
                                {
                                    tick = ((AIComponent)changedComponent).NextActionTick;
                                }

                                break;
                            case (int)EntityComponent.Player:
                                if (changedProperty == nameof(PlayerComponent.NextActionTick))
                                {
                                    tick = (int?)changedValue;
                                }
                                else
                                {
                                    tick = ((PlayerComponent)changedComponent).NextActionTick;
                                }

                                break;
                            case (int)EntityComponent.Effect:
                                if (changedProperty == nameof(EffectComponent.ExpirationTick))
                                {
                                    tick = (int?)changedValue;
                                }
                                else
                                {
                                    tick = ((EffectComponent)changedComponent).ExpirationTick;
                                }

                                break;
                            default:
                                tick = TimeSystem.GetTick(entity);

                                break;
                        }

                        return tick == null ? ((0, 0), false) : ((tick.Value, entity.Id), true);
                    },
                    new PropertyMatcher((int)EntityComponent.AI, nameof(AIComponent.NextActionTick))
                        .With((int)EntityComponent.Player, nameof(PlayerComponent.NextActionTick))
                        .With((int)EntityComponent.Effect, nameof(EffectComponent.ExpirationTick))
                ),
                TickComparer.Instance);

            TimeSystem = new TimeSystem();
            queue.Add(TimeSystem, TimeSystem.AdvanceTurnMessageName, 0);
        }
    }
}
