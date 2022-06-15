using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Time;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack;

public partial class GameManager
{
    public EntityGroup<GameEntity> TemporalEntities
    {
        get;
        private set;
    }

    public SortedUniqueEntityIndex<GameEntity, (int Tick, int Id)> TemporalEntitiesIndex
    {
        get;
        private set;
    }

    public TimeSystem TimeSystem
    {
        get;
        private set;
    }

    private void InitializeTime(SequentialMessageQueue<GameManager> queue)
    {
        TemporalEntities = CreateGroup(nameof(TemporalEntities),
            new EntityMatcher<GameEntity>().AnyOf(
                (int)EntityComponent.AI,
                (int)EntityComponent.Player,
                (int)EntityComponent.Effect,
                (int)EntityComponent.Ability));

        TemporalEntitiesIndex = new SortedUniqueEntityIndex<GameEntity, (int Tick, int Id)>(
            nameof(TemporalEntitiesIndex),
            TemporalEntities,
            new KeyValueGetter<GameEntity, (int Tick, int Id)>(
                (change, matcher, valueType) =>
                {
                    if (matcher.TryGetValue<int?>(
                            change, (int)EntityComponent.Ability, nameof(AbilityComponent.CooldownTick), valueType,
                            out var abilityTick)
                        && abilityTick.HasValue)
                    {
                        return ((abilityTick.Value, change.Entity.Id), true);
                    }

                    if (matcher.TryGetValue<int?>(
                            change, (int)EntityComponent.Effect, nameof(EffectComponent.ExpirationTick), valueType,
                            out var effectTick)
                        && effectTick.HasValue)
                    {
                        return ((effectTick.Value, change.Entity.Id), true);
                    }

                    if (matcher.TryGetValue<int?>(
                            change, (int)EntityComponent.AI, nameof(AIComponent.NextActionTick), valueType,
                            out var aiTick)
                        && aiTick.HasValue)
                    {
                        return ((aiTick.Value, change.Entity.Id), true);
                    }

                    if (matcher.TryGetValue<int?>(
                            change, (int)EntityComponent.Player, nameof(PlayerComponent.NextActionTick), valueType,
                            out var playerTick)
                        && playerTick.HasValue)
                    {
                        return ((playerTick.Value, change.Entity.Id), true);
                    }

                    return ((0, 0), false);
                },
                new PropertyMatcher<GameEntity>()
                    .With(component => ((AIComponent)component).NextActionTick, (int)EntityComponent.AI)
                    .With(component => ((PlayerComponent)component).NextActionTick, (int)EntityComponent.Player)
                    .With(component => ((EffectComponent)component).ExpirationTick, (int)EntityComponent.Effect)
                    .With(component => ((AbilityComponent)component).CooldownTick, (int)EntityComponent.Ability)
            ),
            TickComparer.Instance);

        TimeSystem = new TimeSystem();
        queue.Register(TimeSystem, AdvanceTurnMessage.Name, 0);
    }
}
