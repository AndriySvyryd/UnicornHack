using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack;

public partial class GameManager
{
    public EntityGroup<GameEntity> Effects
    {
        get;
        private set;
    } = null!;

    public CollectionEntityRelationship<GameEntity, HashSet<GameEntity>> EffectsToContainingAbilityRelationship
    {
        get;
        private set;
    } = null!;

    /// <summary>
    ///     Links each ability entity to the affectable entities currently carrying at least one
    ///     <see cref="ActivePropertyEffect" /> it sourced (via <see cref="ActivePropertyEffect.SourceAbilityEntityId" />).
    ///     Maintained by the framework as those instances are added/removed, and used to clean them up when the
    ///     source ability is removed.
    /// </summary>
    public ManyToManyEntityRelationship<GameEntity, ActivePropertyEffect> AppliedEffectsToSourceAbilityRelationship
    {
        get;
        private set;
    } = null!;

    public EngulferToVictimRelationship EngulferToVictimRelationship
    {
        get;
        private set;
    } = null!;

    public EffectApplicationSystem EffectApplicationSystem
    {
        get;
        private set;
    } = null!;

    private void InitializeEffects(SequentialMessageQueue<GameManager> queue)
    {
        Add<EffectComponent>(EntityComponent.Effect, poolSize: 4096, maxPoolSize: 9216);
        Add<ActiveEffectsComponent>(EntityComponent.ActiveEffects, poolSize: 1024, maxPoolSize: 2048);

        Effects = CreateGroup(nameof(Effects),
            new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Effect), capacity: 4096);

        EffectsToContainingAbilityRelationship = new(
            nameof(EffectsToContainingAbilityRelationship),
            Effects,
            Abilities,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((EffectComponent)component).ContainingAbilityId,
                (int)EntityComponent.Effect),
            handlePrincipalDeleted: (effectEntity, _) =>
            {
                if (effectEntity.Effect is { } effect)
                {
                    effect.ContainingAbilityId = null;
                }
            },
            abilityEntity => (HashSet<GameEntity>)abilityEntity.Ability!.Effects,
            effectEntity => effectEntity.Effect!.ContainingAbility,
            keepPrincipalAlive: false, keepDependentAlive: true);

        AppliedEffectsToSourceAbilityRelationship = new(
            nameof(AppliedEffectsToSourceAbilityRelationship),
            AffectableEntities,
            Abilities,
            affectedEntity => affectedEntity.ActiveEffects?.PropertyEffects,
            new KeyValueGetter<GameEntity, int>(
                static (change, _, valueType) =>
                {
                    var changes = change.PropertyChanges;
                    for (var i = 0; i < changes.Count; i++)
                    {
                        if (changes.GetChangedComponent(i).ComponentId != (int)EntityComponent.ActiveEffects
                            || changes.GetChangedPropertyName(i) != nameof(ActiveEffectsComponent.PropertyEffects))
                        {
                            continue;
                        }

                        int? sourceAbilityId;
                        if (changes.GetNestedItem(i) is ActivePropertyEffect effect)
                        {
                            sourceAbilityId = changes.GetNestedPropertyName(i)
                                == nameof(ActivePropertyEffect.SourceAbilityEntityId)
                                    ? changes.GetValue<int?>(i, valueType)
                                    : effect.SourceAbilityEntityId;
                        }
                        else if (changes.GetValue<EntityCollectionChange>(i, valueType).Item
                                 is ActivePropertyEffect collectionEffect)
                        {
                            sourceAbilityId = collectionEffect.SourceAbilityEntityId;
                        }
                        else
                        {
                            continue;
                        }

                        return sourceAbilityId is { } key
                            ? (key, true)
                            : (0, false);
                    }

                    return (0, false);
                },
                new PropertyMatcher<GameEntity>().With(
                    component => ((ActiveEffectsComponent)component).PropertyEffects,
                    (int)EntityComponent.ActiveEffects)),
            (affectedEntity, abilityEntity) =>
                EffectApplicationSystem.HandleSourceAbilityRemoved(affectedEntity, abilityEntity.Id, this));

        EngulferToVictimRelationship = new(AffectableEntities);

        EffectApplicationSystem = new EffectApplicationSystem();
        queue.Register<AbilityActivatedMessage>(EffectApplicationSystem,
            AbilityActivatedMessage.Name, 0);
        queue.Register<EntityAddedMessage<GameEntity>>(EffectApplicationSystem,
            AffectableEntities.GetEntityAddedMessageName(), 0);
        queue.Register<TraveledMessage>(EffectApplicationSystem, TraveledMessage.Name, 2);
    }

}
