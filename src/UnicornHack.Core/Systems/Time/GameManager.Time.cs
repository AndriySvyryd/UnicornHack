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
    } = null!;

    public SortedUniqueEntityIndex<GameEntity, (int Tick, int Id)> TemporalEntitiesIndex
    {
        get;
        private set;
    } = null!;

    public TimeSystem TimeSystem
    {
        get;
        private set;
    } = null!;

    private void InitializeTime(SequentialMessageQueue<GameEntity> queue)
    {
        TemporalEntities = CreateGroup(nameof(TemporalEntities),
            new EntityMatcher<GameEntity>().AnyOf(
                (int)EntityComponent.AI,
                (int)EntityComponent.Player,
                (int)EntityComponent.Ability,
                (int)EntityComponent.ActiveEffects), capacity: 1024);

        TemporalEntitiesIndex = new SortedUniqueEntityIndex<GameEntity, (int Tick, int Id)>(
            nameof(TemporalEntitiesIndex),
            TemporalEntities,
            new KeyValueGetter<GameEntity, (int Tick, int Id)>(
                (change, matcher, valueType) =>
                {
                    // The entity's temporal key is the earliest of its next action / ability cooldown and
                    // its next timed-modifier expiry, so a single slot schedules whichever comes first.
                    int? earliest = null;

                    if (matcher.TryGetValue<int?>(
                            change, (int)EntityComponent.Ability, nameof(AbilityComponent.CooldownTick), valueType,
                            out var abilityTick)
                        && abilityTick.HasValue)
                    {
                        earliest = abilityTick.Value;
                    }

                    if (matcher.TryGetValue<int?>(
                            change, (int)EntityComponent.AI, nameof(AIComponent.NextActionTick), valueType,
                            out var aiTick)
                        && aiTick.HasValue
                        && (earliest == null || aiTick.Value < earliest.Value))
                    {
                        earliest = aiTick.Value;
                    }

                    if (matcher.TryGetValue<int?>(
                            change, (int)EntityComponent.Player, nameof(PlayerComponent.NextActionTick), valueType,
                            out var playerTick)
                        && playerTick.HasValue
                        && (earliest == null || playerTick.Value < earliest.Value))
                    {
                        earliest = playerTick.Value;
                    }

                    if (matcher.TryGetValue<int?>(
                            change, (int)EntityComponent.ActiveEffects,
                            nameof(ActiveEffectsComponent.NextEffectTick), valueType, out var effectTick)
                        && effectTick.HasValue
                        && (earliest == null || effectTick.Value < earliest.Value))
                    {
                        earliest = effectTick.Value;
                    }

                    return earliest.HasValue ? ((earliest.Value, change.Entity.Id), true) : ((0, 0), false);
                },
                new PropertyMatcher<GameEntity>()
                    .With(component => ((AIComponent)component).NextActionTick, (int)EntityComponent.AI)
                    .With(component => ((PlayerComponent)component).NextActionTick, (int)EntityComponent.Player)
                    .With(component => ((AbilityComponent)component).CooldownTick, (int)EntityComponent.Ability)
                    .With(component => ((ActiveEffectsComponent)component).NextEffectTick,
                        (int)EntityComponent.ActiveEffects)
            ),
            TickComparer.Instance);

        TimeSystem = new TimeSystem();
        queue.Register(TimeSystem, AdvanceTurnMessage.Name, 0);
    }
}
