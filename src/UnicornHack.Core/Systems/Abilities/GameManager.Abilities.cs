using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack;

public partial class GameManager
{
    public EntityGroup<GameEntity> Abilities
    {
        get;
        private set;
    } = null!;

    public EntityGroup<GameEntity> AffectableEntities
    {
        get;
        private set;
    } = null!;

    public UniqueEntityIndex<GameEntity, (int, string?)> AffectableAbilitiesIndex
    {
        get;
        private set;
    } = null!;

    public CollectionEntityRelationship<GameEntity, List<GameEntity>> AbilitiesToAffectableRelationship
    {
        get;
        private set;
    } = null!;

    public ManyToManyEntityRelationship<GameEntity, ActiveComponentEffect> ComponentEffectsToCarrierRelationship
    {
        get;
        private set;
    } = null!;

    public AbilityActivationSystem AbilityActivationSystem
    {
        get;
        private set;
    } = null!;

    public AbilitySlottingSystem AbilitySlottingSystem
    {
        get;
        private set;
    } = null!;

    private void InitializeAbilities(SequentialMessageQueue<GameManager> queue)
    {
        Add<AbilityComponent>(EntityComponent.Ability, poolSize: 4096, maxPoolSize: 8192);

        AffectableEntities = CreateGroup(nameof(AffectableEntities),
            new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.ActiveEffects), capacity: 1024);

        Abilities = CreateGroup(nameof(Abilities),
            new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Ability), capacity: 4096);

        AffectableAbilitiesIndex = new(
            nameof(AffectableAbilitiesIndex),
            Abilities,
            new KeyValueGetter<GameEntity, (int, string?)>(
                (change, matcher, valueType) =>
                {
                    if (!matcher.TryGetValue<int?>(
                            change, (int)EntityComponent.Ability, nameof(AbilityComponent.OwnerId),
                            valueType, out var ownerId)
                        || !ownerId.HasValue
                        || !matcher.TryGetValue<string?>(
                            change, (int)EntityComponent.Ability, nameof(AbilityComponent.Name),
                            valueType, out var name)
                        || name == null)
                    {
                        return ((0, null), false);
                    }

                    return ((ownerId.Value, name), true);
                },
                new PropertyMatcher<GameEntity>()
                    .With(component => ((AbilityComponent)component).OwnerId, (int)EntityComponent.Ability)
                    .With(component => ((AbilityComponent)component).Name, (int)EntityComponent.Ability)
            ));

        AbilitiesToAffectableRelationship = new(
            nameof(AbilitiesToAffectableRelationship),
            Abilities,
            AffectableEntities,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((AbilityComponent)component).OwnerId,
                (int)EntityComponent.Ability),
            (effectEntity, _) => {},
            containerEntity => (List<GameEntity>)containerEntity.ActiveEffects!.Abilities,
            keepPrincipalAlive: false,
            keepDependentAlive: true);

        // Structural component effects own their carrier abilities through CarrierId. Removing the last
        // record removes the carrier component; removing the carrier component removes every referencing record.
        ComponentEffectsToCarrierRelationship = new(
            nameof(ComponentEffectsToCarrierRelationship),
            AffectableEntities,
            Abilities,
            affectedEntity => affectedEntity.ActiveEffects?.ComponentEffects,
            new KeyValueGetter<GameEntity, int>(
                static (change, _, valueType) =>
                {
                    var changes = change.PropertyChanges;
                    for (var i = 0; i < changes.Count; i++)
                    {
                        if (changes.GetChangedComponent(i).ComponentId != (int)EntityComponent.ActiveEffects
                            || changes.GetChangedPropertyName(i) != nameof(ActiveEffectsComponent.ComponentEffects))
                        {
                            continue;
                        }

                        int carrierId;
                        if (changes.GetNestedItem(i) is ActiveComponentEffect effect)
                        {
                            var nestedPropertyName = changes.GetNestedPropertyName(i);
                            carrierId = nestedPropertyName == nameof(ActiveComponentEffect.CarrierId)
                                ? changes.GetValue<int>(i, valueType)
                                : effect.CarrierId;
                        }
                        else if (changes.GetValue<EntityCollectionChange>(i, valueType).Item
                                 is ActiveComponentEffect collectionEffect)
                        {
                            carrierId = collectionEffect.CarrierId;
                        }
                        else
                        {
                            continue;
                        }

                        return (carrierId, true);
                    }

                    return (0, false);
                },
                new PropertyMatcher<GameEntity>().With(
                    component => ((ActiveEffectsComponent)component).ComponentEffects,
                    (int)EntityComponent.ActiveEffects)),
            (owningBeing, carrier) =>
            {
                var effects = owningBeing.ActiveEffects?.ComponentEffects;
                if (effects == null)
                {
                    return;
                }

                for (var i = effects.Count - 1; i >= 0; i--)
                {
                    if (effects[i].CarrierId == carrier.Id)
                    {
                        effects.RemoveAt(i);
                    }
                }
            },
            (_, carrier) => carrier.RemoveComponent((int)EntityComponent.Ability),
            getPrincipalRemovedComponent: carrier => carrier.Ability);

        AbilityActivationSystem = new AbilityActivationSystem();
        queue.Register<ActivateAbilityMessage>(AbilityActivationSystem,
            ActivateAbilityMessage.Name, 0);
        queue.Register<DeactivateAbilityMessage>(AbilityActivationSystem,
            DeactivateAbilityMessage.Name, 0);
        queue.Register<ItemEquippedMessage>(AbilityActivationSystem, ItemEquippedMessage.Name, 0);
        queue.Register<DiedMessage>(AbilityActivationSystem, DiedMessage.Name, 2);
        queue.Register<EntityAddedMessage<GameEntity>>(
            AbilityActivationSystem, AbilitiesToAffectableRelationship.Dependents.GetEntityAddedMessageName(), 0);
        queue.Register<EntityRemovedMessage<GameEntity>>(
            AbilityActivationSystem, AbilitiesToAffectableRelationship.Dependents.GetEntityRemovedMessageName(), 0);
        queue.Register<PropertyValueChangedMessage<GameEntity, int>>(
            AbilityActivationSystem,
            Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.CooldownFactor)), 0);

        AbilitySlottingSystem = new AbilitySlottingSystem();
        queue.Register<SetAbilitySlotMessage>(AbilitySlottingSystem,
            SetAbilitySlotMessage.Name, 0);
        queue.Register<PropertyValueChangedMessage<GameEntity, bool>>(AbilitySlottingSystem,
            AbilitiesToAffectableRelationship.Dependents.GetPropertyValueChangedMessageName(
                nameof(AbilityComponent.IsUsable)),
            0);
        queue.Register<PropertyValueChangedMessage<GameEntity, int>>(AbilitySlottingSystem,
            Beings.GetPropertyValueChangedMessageName(nameof(PhysicalComponent.Capacity)),
            0);
        queue.Register<EntityAddedMessage<GameEntity>>(
            AbilitySlottingSystem, AbilitiesToAffectableRelationship.Dependents.GetEntityAddedMessageName(), 2);
    }
}
