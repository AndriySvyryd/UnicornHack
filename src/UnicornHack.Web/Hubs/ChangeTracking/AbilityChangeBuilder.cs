using System.Collections;
using UnicornHack.Services;
using UnicornHack.Systems.Abilities;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs.ChangeTracking;

public class AbilityChangeBuilder : ChangeBuilder<AbilityChange>
{
    private readonly int _playerEntityId;

    public AbilityChangeBuilder(int playerEntityId)
    {
        _playerEntityId = playerEntityId;
    }

    public override void RegisterOnGroups(GameManager manager)
    {
        manager.Abilities.AddListener(this);
        manager.AbilitiesToAffectableRelationship.AddDependentsListener(this);
    }

    public override void UnregisterFromGroups(GameManager manager)
    {
        manager.Abilities.RemoveListener(this);
        manager.AbilitiesToAffectableRelationship.RemoveDependentsListener(this);
    }

    protected override bool IsRelevantPrincipal(GameEntity principal)
        => principal.Id == _playerEntityId;

    // An ability is "visible to the client" iff it has a slot.
    protected override bool IsRelevant(GameEntity entity)
        => entity.Ability?.Slot != null;

    protected override bool IsRelevantOnRemove(in EntityChange<GameEntity> entityChange)
        => entityChange.RemovedComponent is AbilityComponent ability && ability.Slot != null;

    /// <summary>
    ///     Property-change handling must also detect <c>Slot</c> transitions, because slot
    ///     presence is what determines client visibility — the relationship events only
    ///     fire on ownership changes.
    /// </summary>
    protected override void OnBaseGroupPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
    {
        var entity = entityChange.Entity;
        if (entity.Ability?.OwnerId != _playerEntityId)
        {
            return;
        }

        var changes = entityChange.PropertyChanges;
        int? oldSlot = null;
        var hasSlotPropertyChange = false;
        for (var i = 0; i < changes.Count; i++)
        {
            if (changes.GetChangedComponent(i).ComponentId == (int)EntityComponent.Ability
                && changes.GetChangedPropertyName(i) == nameof(AbilityComponent.Slot))
            {
                hasSlotPropertyChange = true;
                oldSlot = changes.GetValue<int?>(i, Utils.MessagingECS.ValueType.Old);
                break;
            }
        }

        var currentSlot = entity.Ability!.Slot;

        if (hasSlotPropertyChange && currentSlot == null && !Changes.ContainsKey(entity.Id))
        {
            // Slot cleared, not tracked this turn → entity leaves the client's view.
            Debug.Assert(oldSlot != null,
                "Ability not yet tracked but slot has just been cleared — oldSlot must have been non-null "
                + "for the client to have known this ability.");
            // HandleRemoved will capture a snapshot; capture it now using Old slot value.
            EnsureSnapshot(entity, changes);
            HandleRemoved(entity);
            return;
        }

        if (currentSlot == null && !Changes.ContainsKey(entity.Id))
        {
            // Not visible and not tracked — nothing to do.
            return;
        }

        var tracked = GetOrCreateChange(entity);

        if (hasSlotPropertyChange)
        {
            if (currentSlot == null)
            {
                // Slot cleared on a tracked entity.
                if (tracked.State == EntityState.Added && !Snapshots.ContainsKey(entity.Id))
                {
                    // Add-then-Remove on a brand-new ability within this cycle: cancel.
                    // The client never knew this ability — emitting a removal sentinel
                    // would target an unknown id.
                    Changes.Remove(entity.Id);
                    return;
                }

                // Capture snapshot if not yet present (oldSlot was non-null, so the client
                // knew this ability).
                if (oldSlot != null)
                {
                    EnsureSnapshot(entity, changes);
                }

                tracked.State = EntityState.Deleted;
            }
            else if (oldSlot == null)
            {
                // Slot just went null → non-null. The client did not know this ability
                // before this cycle (slot was null at cycle start), so the dependent
                // listener's Property event may have spuriously captured a snapshot —
                // drop it. Force Added unless we're re-restoring a prior in-cycle Deleted.
                var snapshot = Snapshots.TryGetValue(entity.Id, out var s) ? s : null;
                if (tracked.State is EntityState.Deleted && snapshot is { Slot: not null })
                {
                    // Genuine re-Add: the snapshot was captured from a non-null slot earlier
                    // in this cycle. Reconcile bits accumulated during the deleted phase.
                    OnReconcileAfterReAdd(entity, tracked);
                    tracked.State = EntityState.Modified;
                }
                else
                {
                    Snapshots.Remove(entity.Id);
                    tracked.State = EntityState.Added;
                }
            }
        }
    }

    protected override void TrackPropertyChanges(
        GameEntity entity, AbilityChange change, IPropertyValueChanges changes)
    {
        for (var i = 0; i < changes.Count; i++)
        {
            if (changes.GetChangedComponent(i).ComponentId != (int)EntityComponent.Ability)
            {
                continue;
            }

            switch (changes.GetChangedPropertyName(i))
            {
                case nameof(AbilityComponent.Name):
                    change.Name = entity.Game.Services.Language.GetString(entity.Ability!);
                    break;
                case nameof(AbilityComponent.Activation):
                    change.Activation = (int)entity.Ability!.Activation;
                    break;
                case nameof(AbilityComponent.Slot):
                    change.Slot = entity.Ability!.Slot;
                    break;
                case nameof(AbilityComponent.CooldownTick):
                    change.CooldownTick = entity.Ability!.CooldownTick;
                    break;
                case nameof(AbilityComponent.CooldownXpLeft):
                    change.CooldownXpLeft = entity.Ability!.CooldownXpLeft;
                    break;
                case nameof(AbilityComponent.TargetingShape):
                    change.TargetingShape = (int)entity.Ability!.TargetingShape;
                    break;
                case nameof(AbilityComponent.TargetingShapeSize):
                    change.TargetingShapeSize = entity.Ability!.TargetingShapeSize;
                    break;
            }
        }
    }

    protected override AbilityChange ProduceFullSnapshot(GameEntity entity, SerializationContext context)
        => SerializeAbility(entity, context.Services);

    /// <summary>
    ///     Captures the entity's pre-cycle state. Uses <c>Old</c> values from
    ///     <paramref name="changes" /> for properties that just changed and the entity's
    ///     current values for the rest — the result represents what the client last saw at
    ///     the start of the cycle.
    ///     <para>
    ///         When invoked from <see cref="ChangeBuilder{TChange}.HandleRemoved" /> via the
    ///         dependent-relationship Remove path, the <see cref="AbilityComponent" /> has
    ///         already been removed from the entity. In that case there is no in-cycle
    ///         re-Add path that needs reconciliation, so returning <c>null</c> is safe — the
    ///         snapshot key still records "client knew this entity".
    ///     </para>
    /// </summary>
    protected override AbilityChange? CaptureSnapshot(GameEntity entity, IPropertyValueChanges? changes)
    {
        var ability = entity.Ability;
        if (ability == null)
        {
            return null;
        }

        var snapshot = new AbilityChange
        {
            ChangedProperties = null,
            Activation = (int)ability.Activation,
            Slot = ability.Slot,
            CooldownTick = ability.CooldownTick,
            CooldownXpLeft = ability.CooldownXpLeft,
            TargetingShape = (int)ability.TargetingShape,
            TargetingShapeSize = ability.TargetingShapeSize
        };

        if (changes == null)
        {
            return snapshot;
        }

        for (var i = 0; i < changes.Count; i++)
        {
            if (changes.GetChangedComponent(i).ComponentId != (int)EntityComponent.Ability)
            {
                continue;
            }

            switch (changes.GetChangedPropertyName(i))
            {
                case nameof(AbilityComponent.Activation):
                    snapshot.Activation = (int)changes.GetValue<ActivationType>(
                        i, Utils.MessagingECS.ValueType.Old);
                    break;
                case nameof(AbilityComponent.Slot):
                    snapshot.Slot = changes.GetValue<int?>(i, Utils.MessagingECS.ValueType.Old);
                    break;
                case nameof(AbilityComponent.CooldownTick):
                    snapshot.CooldownTick = changes.GetValue<int?>(i, Utils.MessagingECS.ValueType.Old);
                    break;
                case nameof(AbilityComponent.CooldownXpLeft):
                    snapshot.CooldownXpLeft = changes.GetValue<int?>(
                        i, Utils.MessagingECS.ValueType.Old);
                    break;
                case nameof(AbilityComponent.TargetingShape):
                    snapshot.TargetingShape = (int)changes.GetValue<TargetingShape>(
                        i, Utils.MessagingECS.ValueType.Old);
                    break;
                case nameof(AbilityComponent.TargetingShapeSize):
                    snapshot.TargetingShapeSize = changes.GetValue<int>(
                        i, Utils.MessagingECS.ValueType.Old);
                    break;
            }
        }

        return snapshot;
    }

    /// <summary>
    ///     After a transient Deleted → Added cycle within one turn, the tracked change has
    ///     accumulated bits from both phases. Compare each property against the snapshot
    ///     captured at the start of the cycle (what the client last saw) and clear bits
    ///     where the value round-tripped.
    /// </summary>
    protected override void OnReconcileAfterReAdd(GameEntity entity, AbilityChange tracked)
    {
        var ability = entity.Ability!;
        if (tracked.ChangedProperties == null
            || tracked.ChangedProperties.Length == 1)
        {
            tracked.ChangedProperties = new BitArray(AbilityChange.PropertyCount);
        }

        var bits = tracked.ChangedProperties;
        var services = entity.Game.Services;

        if (!Snapshots.TryGetValue(entity.Id, out var snapshot) || snapshot == null)
        {
            return;
        }

        if (bits[2] && tracked.Activation == snapshot.Activation)
        {
            bits[2] = false;
        }

        if (bits[3] && tracked.Slot == snapshot.Slot)
        {
            bits[3] = false;
        }

        if (bits[4] && tracked.CooldownTick == snapshot.CooldownTick)
        {
            bits[4] = false;
        }

        if (bits[5] && tracked.CooldownXpLeft == snapshot.CooldownXpLeft)
        {
            bits[5] = false;
        }

        if (bits[6] && tracked.TargetingShape == snapshot.TargetingShape)
        {
            bits[6] = false;
        }

        if (bits[7] && tracked.TargetingShapeSize == snapshot.TargetingShapeSize)
        {
            bits[7] = false;
        }
    }

    public static AbilityChange SerializeAbility(GameEntity abilityEntity, GameServices services)
    {
        var ability = abilityEntity.Ability!;
        return new AbilityChange
        {
            ChangedProperties = null,
            Name = services.Language.GetString(ability),
            Activation = (int)ability.Activation,
            Slot = ability.Slot,
            CooldownTick = ability.CooldownTick,
            CooldownXpLeft = ability.CooldownXpLeft,
            TargetingShape = (int)ability.TargetingShape,
            TargetingShapeSize = ability.TargetingShapeSize
        };
    }
}
