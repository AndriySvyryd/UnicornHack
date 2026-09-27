using UnicornHack.Systems.Items;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities;

public class AvailableAbilityRelationship :
    CollectionEntityRelationship<GameEntity, HashSet<GameEntity>>,
    IDependentEntityChangeListener<GameEntity>
{
    public AvailableAbilityRelationship(
        IEntityGroup<GameEntity> abilities,
        IEntityGroup<GameEntity> actors,
        CollectionEntityRelationship<GameEntity, HashSet<GameEntity>> itemsToContainerRelationship)
        : base(
            nameof(GameManager.AvailableAbilitiesToRoomActorsRelationship),
            abilities,
            actors,
            new KeyValueGetter<GameEntity, int>(
                (change, matcher, valueType) =>
                {
                    if (!matcher.TryGetValue<int?>(
                            change, (int)EntityComponent.Ability, nameof(AbilityComponent.OwnerId),
                            valueType, out var ownerId)
                        || ownerId == null
                        || (change.Entity.Ability ?? change.RemovedComponent as AbilityComponent) is not { } ability
                        || ResolvePrincipal(change.Entity, ability, ownerId.Value, actors) is not { } actor)
                    {
                        return (0, false);
                    }

                    return (actor.Id, true);
                },
                new PropertyMatcher<GameEntity>().With(
                    component => ((AbilityComponent)component).OwnerId,
                    (int)EntityComponent.Ability)),
            (abilityEntity, _) =>
            {
                if (abilityEntity.Ability is { } ability)
                {
                    ability.Slot = null;
                }
            },
            actorEntity => (HashSet<GameEntity>)actorEntity.ActiveEffects!.AvailableAbilities)
    {
        itemsToContainerRelationship.AddDependentsListener(this);
    }

    public GameEntity? GetSlottableItemAbility(GameEntity itemEntity, GameEntity actorEntity)
    {
        GameEntity? slottableAbility = null;
        var availableAbilities = actorEntity.ActiveEffects!.AvailableAbilities;
        foreach (var abilityEntity in itemEntity.ActiveEffects?.Abilities ?? [])
        {
            if ((abilityEntity.Ability!.Activation & ActivationType.Slottable) == 0
                || !availableAbilities.Contains(abilityEntity))
            {
                continue;
            }

            Debug.Assert(slottableAbility == null,
                $"Item {itemEntity.Id} exposes more than one slottable ability.");
            slottableAbility = abilityEntity;
        }

        return slottableAbility;
    }

    protected override bool TryRemoveEntity(int key, in EntityChange<GameEntity> entityChange)
    {
        var removed = base.TryRemoveEntity(key, entityChange);
        if (removed
            && entityChange.Entity.Ability is { Slot: not null } ability)
        {
            ability.Slot = null;
        }

        return removed;
    }

    protected override void HandleNonKeyPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
    {
        if (entityChange.Entity.Ability is not { OwnerId: { } ownerId } ability
            || ResolvePrincipal(entityChange.Entity, ability, ownerId, PrincipalGroup) == null)
        {
            return;
        }

        base.HandleNonKeyPropertyValuesChanged(entityChange);
    }

    private static GameEntity? ResolvePrincipal(
        GameEntity abilityEntity, AbilityComponent ability, int ownerId,
        IEntityGroup<GameEntity> actors)
    {
        if (abilityEntity.Manager.FindEntity(ownerId) is not { } owner)
        {
            return null;
        }

        GameEntity? actor;
        if (owner.Being != null)
        {
            actor = owner;
        }
        else if (owner.Item is { } item
                 && IsItemAbilityAvailable(ability, item))
        {
            actor = FindContainingActor(owner, abilityEntity.Manager);
        }
        else
        {
            return null;
        }

        return actor != null
               && actors.ContainsEntity(actor.Id)
            ? actor
            : null;
    }

    void IDependentEntityChangeListener<GameEntity>.OnEntityAdded(
        in EntityChange<GameEntity> entityChange, GameEntity principal)
    {
        if (principal.Being != null)
        {
            RefreshItemAbilities(entityChange.Entity, principal);
        }
    }

    void IDependentEntityChangeListener<GameEntity>.OnEntityRemoved(
        in EntityChange<GameEntity> entityChange, GameEntity principal)
    {
        if (principal.Being == null)
        {
            return;
        }

        foreach (var abilityEntity in entityChange.Entity.ActiveEffects?.Abilities ?? [])
        {
            TryRemoveEntity(principal.Id, new EntityChange<GameEntity>(abilityEntity));
        }
    }

    void IDependentEntityChangeListener<GameEntity>.OnPropertyValuesChanged(
        in EntityChange<GameEntity> entityChange, GameEntity principal)
    {
        if (principal.Being != null)
        {
            RefreshItemAbilities(entityChange.Entity, principal);
        }
    }

    private void RefreshItemAbilities(GameEntity itemEntity, GameEntity actorEntity)
    {
        var availableAbilities = actorEntity.ActiveEffects!.AvailableAbilities;
        int? previousSlot = null;
        foreach (var abilityEntity in itemEntity.ActiveEffects?.Abilities ?? [])
        {
            if (availableAbilities.Contains(abilityEntity)
                && !IsItemAbilityAvailable(abilityEntity.Ability!, itemEntity.Item!))
            {
                if ((abilityEntity.Ability!.Activation & ActivationType.Slottable) != 0)
                {
                    previousSlot ??= abilityEntity.Ability.Slot;
                }

                TryRemoveEntity(actorEntity.Id, new EntityChange<GameEntity>(abilityEntity));
            }
        }

        foreach (var abilityEntity in itemEntity.ActiveEffects?.Abilities ?? [])
        {
            if (!availableAbilities.Contains(abilityEntity)
                && IsItemAbilityAvailable(abilityEntity.Ability!, itemEntity.Item!))
            {
                TryAddEntity(actorEntity.Id, new EntityChange<GameEntity>(abilityEntity));
            }
        }

        var slottableAbility = GetSlottableItemAbility(itemEntity, actorEntity);
        if (previousSlot != null
            && slottableAbility?.Ability is { Slot: null } ability)
        {
            ability.Slot = previousSlot;
        }
    }

    private static bool IsItemAbilityAvailable(AbilityComponent ability, ItemComponent item)
    {
        if (ability.Template == null
            && ability.Name == "Equip")
        {
            return item.EquippedSlot == EquipmentSlot.None;
        }

        return item.EquipableSlots == EquipmentSlot.None
               || item.EquippedSlot != EquipmentSlot.None;
    }

    private static GameEntity? FindContainingActor(GameEntity owner, GameManager manager)
    {
        var container = owner;
        while (container.Item?.ContainerId is { } containerId)
        {
            if (manager.FindEntity(containerId) is not { } nextContainer
                || nextContainer == container)
            {
                return null;
            }

            container = nextContainer;
        }

        return container.Being != null ? container : null;
    }
}