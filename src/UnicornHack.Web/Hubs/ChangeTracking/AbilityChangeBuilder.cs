using System.Collections;
using UnicornHack.Generation.Effects;
using UnicornHack.Services;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs.ChangeTracking;

public class AbilityChangeBuilder : ChangeBuilder<AbilityChange>
{
    private readonly int _playerEntityId;
    private readonly Dictionary<int, int> _accumulatorPlaceholders = [];
    private bool _accumulatorsChanged;

    public AbilityChangeBuilder(int playerEntityId)
    {
        _playerEntityId = playerEntityId;
    }

    public bool HasAccumulatorChanges => _accumulatorsChanged;

    public override void RegisterOnGroups(GameManager manager)
    {
        manager.Abilities.AddListener(this);
        manager.AffectableEntities.AddListener(this);
        manager.AbilitiesToAffectableRelationship.AddDependentsListener(this);
    }

    public override void UnregisterFromGroups(GameManager manager)
    {
        manager.Abilities.RemoveListener(this);
        manager.AffectableEntities.RemoveListener(this);
        manager.AbilitiesToAffectableRelationship.RemoveDependentsListener(this);
    }

    protected override bool IsRelevantPrincipal(GameEntity principal)
        => principal.Id == _playerEntityId;

    // Client-visible abilities have a slot or are slot-less status-like/race carriers. Status-like abilities
    // drive the HUD; races are included so generic ability transport can replace the dedicated race DTO.
    protected override bool IsRelevant(GameEntity entity)
        => entity.Ability is { } ability
            && ability.OwnerId == _playerEntityId
            && !ability.IsLeveledSupplement
            && IsClientVisible(ability);

    protected override bool IsRelevantOnRemove(in EntityChange<GameEntity> entityChange)
        => entityChange.RemovedComponent is AbilityComponent ability
            && ability.OwnerId == _playerEntityId
            && !ability.IsLeveledSupplement
            && IsClientVisible(ability);

    /// <summary>
    ///     Property-change handling must also detect <c>Slot</c> transitions, because slot
    ///     presence is what determines client visibility — the relationship events only
    ///     fire on ownership changes.
    /// </summary>
    protected override void OnBaseGroupPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
    {
        var entity = entityChange.Entity;
        if (entity.Id == _playerEntityId
            && entity.ActiveEffects != null)
        {
            TrackAccumulatorChanges(entityChange);
            TrackComponentEffectChanges(entityChange);
            return;
        }

        if (entity.Ability?.OwnerId != _playerEntityId)
        {
            return;
        }

        var changes = entityChange.PropertyChanges;
        int? oldSlot = null;
        var hasSlotPropertyChange = false;
        for (var i = 0; i < changes.Count; i++)
        {
            if (changes.GetChangedComponent(i).ComponentId != (int)EntityComponent.Ability)
            {
                continue;
            }

            if (changes.GetChangedPropertyName(i) == nameof(AbilityComponent.Slot))
            {
                hasSlotPropertyChange = true;
                oldSlot = changes.GetValue<int?>(i, Utils.MessagingECS.ValueType.Old);
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

    private void TrackComponentEffectChanges(in EntityChange<GameEntity> entityChange)
    {
        var changes = entityChange.PropertyChanges;
        for (var i = 0; i < changes.Count; i++)
        {
            if (changes.GetChangedComponent(i).ComponentId != (int)EntityComponent.ActiveEffects
                || changes.GetChangedPropertyName(i) != nameof(ActiveEffectsComponent.ComponentEffects))
            {
                continue;
            }

            var record = changes.GetNestedItem(i) as ActiveComponentEffect
                         ?? changes.GetValue<EntityCollectionChange>(
                             i, Utils.MessagingECS.ValueType.Current).Item as ActiveComponentEffect
                         ?? changes.GetValue<EntityCollectionChange>(
                             i, Utils.MessagingECS.ValueType.Old).Item as ActiveComponentEffect;
            TrackComponentEffectRecord(record, entityChange.Entity);
        }
    }

    private void TrackComponentEffectRecord(ActiveComponentEffect? record, GameEntity ownerEntity)
    {
        if (record is { EffectType: EffectType.DecreaseAbilityLevel })
        {
            TrackDecreasedAbilities(record, ownerEntity);
            return;
        }

        if (record is not
            {
                EffectType: EffectType.AddAbility,
                TargetName: { } addedName
            }
            || ownerEntity.Manager.AffectableAbilitiesIndex[(ownerEntity.Id, addedName)] is not
                { Ability: { } ability } abilityEntity
            || !ability.Type.IsStatusLike() && ability.Type != AbilityType.Race)
        {
            return;
        }

        var change = GetOrCreateChange(abilityEntity, EntityState.Modified);
        change.Level = Math.Max(0, EffectApplicationSystem.GetAbilityLevel(ownerEntity, addedName));
        if (ability.Type.IsStatusLike())
        {
            change.ExpirationTarget = record.ExpirationTarget;
            change.ExpirationEvent = GetExpirationEvent(record);
        }
    }

    private void TrackDecreasedAbilities(ActiveComponentEffect decrease, GameEntity ownerEntity)
    {
        if (decrease.ConditionValue is not { } conditionValue)
        {
            if (ownerEntity.Manager.AffectableAbilitiesIndex[
                (ownerEntity.Id, decrease.TargetName!)] is { Ability: { } ability } abilityEntity
                && IsClientVisible(ability))
            {
                TrackEffectiveLevel(abilityEntity, ownerEntity);
            }

            return;
        }

        var targetType = (AbilityType)conditionValue;

        foreach (var abilityEntity in ownerEntity.ActiveEffects!.Abilities)
        {
            if (abilityEntity.Id == decrease.SourceAbilityEntityId
                || abilityEntity.Ability is not { Type: var abilityType } ability
                || abilityType != targetType
                || ability.IsLeveledSupplement
                || !IsClientVisible(ability))
            {
                continue;
            }

            TrackEffectiveLevel(abilityEntity, ownerEntity);
        }
    }

    private void TrackEffectiveLevel(GameEntity abilityEntity, GameEntity ownerEntity)
    {
        var abilityName = abilityEntity.Ability!.Name!;
        var change = GetOrCreateChange(abilityEntity, EntityState.Modified);
        change.Level = Math.Max(0, EffectApplicationSystem.GetAbilityLevel(ownerEntity, abilityName));
        change.LevelReduction = EffectApplicationSystem.GetAbilityLevelReduction(ownerEntity, abilityName);
    }

    internal static bool IsClientVisible(AbilityComponent ability)
        => ability.Slot != null
           || ability.Type.IsStatusLike()
           || ability.Type == AbilityType.Race;

    private void TrackAccumulatorChanges(in EntityChange<GameEntity> entityChange)
    {
        var changes = entityChange.PropertyChanges;
        for (var i = 0; i < changes.Count; i++)
        {
            if (changes.GetChangedComponent(i).ComponentId != (int)EntityComponent.ActiveEffects
                || changes.GetChangedPropertyName(i) != nameof(ActiveEffectsComponent.Accumulators))
            {
                continue;
            }

            if (changes.GetNestedItem(i) is EffectAccumulator changed)
            {
                switch (changes.GetNestedPropertyName(i))
                {
                    case nameof(EffectAccumulator.EffectType):
                        TrackAccumulator(
                            changes.GetValue<EffectType>(i, Utils.MessagingECS.ValueType.Old),
                            entityChange.Entity);
                        TrackAccumulator(changed.EffectType, entityChange.Entity);
                        break;
                    case nameof(EffectAccumulator.Value):
                        TrackAccumulator(changed.EffectType, entityChange.Entity);
                        break;
                }

                continue;
            }

            var oldChange = changes.GetValue<EntityCollectionChange>(i, Utils.MessagingECS.ValueType.Old);
            var newChange = changes.GetValue<EntityCollectionChange>(i, Utils.MessagingECS.ValueType.Current);
            if (newChange is { Action: EntityCollectionChangeAction.Add, Item: EffectAccumulator added })
            {
                TrackAccumulator(added.EffectType, entityChange.Entity);
            }

            if (oldChange is { Action: EntityCollectionChangeAction.Remove, Item: EffectAccumulator removed })
            {
                TrackAccumulator(removed.EffectType, entityChange.Entity);
            }
        }
    }

    private void TrackAccumulator(EffectType effectType, GameEntity ownerEntity)
    {
        _accumulatorsChanged = true;
        var metadata = Effect.GetMetadata(effectType);
        if (metadata.StatusAbilityName is not { } statusAbilityName
            || ownerEntity.Manager.AffectableAbilitiesIndex[
                (_playerEntityId, statusAbilityName)] is not { } abilityEntity)
        {
            return;
        }

        GetOrCreateChange(abilityEntity, EntityState.Modified).Accumulator = GetAccumulator(abilityEntity);
    }

    public void WriteAccumulatorPlaceholdersTo(
        Dictionary<int, AbilityChange> output, GameEntity ownerEntity)
    {
        var current = GetAccumulatorPlaceholders(ownerEntity);
        foreach (var (id, placeholder) in current)
        {
            if (!_accumulatorPlaceholders.TryGetValue(id, out var previousValue))
            {
                output[id] = placeholder;
            }
            else if (previousValue != placeholder.Accumulator)
            {
                output[id] = new AbilityChange { Accumulator = placeholder.Accumulator };
            }
        }

        foreach (var id in _accumulatorPlaceholders.Keys)
        {
            if (!current.ContainsKey(id))
            {
                output[id] = new AbilityChange { ChangedProperties = SerializationContext.DeletedBitArray };
            }
        }
    }

    public void ResetAccumulatorPlaceholders(GameEntity ownerEntity)
    {
        _accumulatorPlaceholders.Clear();
        foreach (var (id, placeholder) in GetAccumulatorPlaceholders(ownerEntity))
        {
            _accumulatorPlaceholders.Add(id, placeholder.Accumulator!.Value);
        }

        _accumulatorsChanged = false;
    }

    public static Dictionary<int, AbilityChange> GetAccumulatorPlaceholders(GameEntity ownerEntity)
    {
        var placeholders = new Dictionary<int, AbilityChange>();
        foreach (var accumulator in ownerEntity.ActiveEffects!.Accumulators)
        {
            var metadata = Effect.GetMetadata(accumulator.EffectType);
            if (metadata.StatusAbilityName is not { } abilityName
                || ownerEntity.Manager.AffectableAbilitiesIndex[(ownerEntity.Id, abilityName)] != null)
            {
                continue;
            }

            var id = GetAccumulatorPlaceholderId(accumulator.EffectType);
            placeholders.Add(id, new AbilityChange
            {
                ChangedProperties = null,
                Name = GetAccumulatorPlaceholderName(accumulator.EffectType),
                Activation = (int)ActivationType.Always,
                Type = (int)AbilityType.Status,
                Accumulator = accumulator.Value,
                Level = 0
            });
        }

        return placeholders;
    }

    public static int GetAccumulatorPlaceholderId(EffectType effectType) => -1 - (int)effectType;

    public static string GetAccumulatorPlaceholderName(EffectType effectType)
        => Capitalize(Effect.GetMetadata(effectType).StatusAbilityName!);

    private static string Capitalize(string value)
        => value.Length == 0 ? value : char.ToUpperInvariant(value[0]) + value[1..];

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
                case nameof(AbilityComponent.Level):
                    change.Level = entity.Ability!.Level;
                    break;
                case nameof(AbilityComponent.Noise):
                    change.Noise = entity.Ability!.Noise;
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

        var statusRecord = GetStatusRecord(ability.Entity);
        var snapshot = new AbilityChange
        {
            ChangedProperties = null,
            Activation = (int)ability.Activation,
            Slot = ability.Slot,
            CooldownTick = ability.CooldownTick,
            CooldownXpLeft = ability.CooldownXpLeft,
            TargetingShape = (int)ability.TargetingShape,
            TargetingShapeSize = ability.TargetingShapeSize,
            Type = (int)ability.Type,
            Accumulator = GetAccumulator(ability.Entity),
            Level = ability.Level,
            LevelReduction = GetLevelReduction(ability.Entity),
            ExpirationTarget = statusRecord?.ExpirationTarget,
            Noise = ability.Noise,
            ExpirationEvent = GetExpirationEvent(statusRecord)
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
                case nameof(AbilityComponent.Noise):
                    snapshot.Noise = changes.GetValue<int?>(i, Utils.MessagingECS.ValueType.Old);
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
        if (tracked.ChangedProperties == null
            || tracked.ChangedProperties.Length == 1)
        {
            tracked.ChangedProperties = new BitArray(AbilityChange.PropertyCount);
        }

        var bits = tracked.ChangedProperties;

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

        if (bits[10] && tracked.Level == snapshot.Level)
        {
            bits[10] = false;
        }

        if (bits[11] && tracked.LevelReduction == snapshot.LevelReduction)
        {
            bits[11] = false;
        }

        if (bits[12] && tracked.ExpirationTarget == snapshot.ExpirationTarget)
        {
            bits[12] = false;
        }

        if (bits[13] && tracked.ExpirationEvent == snapshot.ExpirationEvent)
        {
            bits[13] = false;
        }

        if (bits[14] && tracked.Noise == snapshot.Noise)
        {
            bits[14] = false;
        }
    }

    public static AbilityChange SerializeAbility(GameEntity abilityEntity, GameServices services)
    {
        var ability = abilityEntity.Ability!;
        var statusRecord = GetStatusRecord(abilityEntity);
        return new AbilityChange
        {
            ChangedProperties = null,
            Name = ability.Type == AbilityType.Race
                ? services.Language.GetTemplateName(ability, abbreviate: false)
                : services.Language.GetString(ability),
            Activation = (int)ability.Activation,
            Slot = ability.Slot,
            CooldownTick = ability.CooldownTick,
            CooldownXpLeft = ability.CooldownXpLeft,
            TargetingShape = (int)ability.TargetingShape,
            TargetingShapeSize = ability.TargetingShapeSize,
            Type = (int)ability.Type,
            Accumulator = GetAccumulator(abilityEntity),
            Level = ability.Level,
            LevelReduction = GetLevelReduction(abilityEntity),
            ExpirationTarget = statusRecord?.ExpirationTarget,
            Noise = ability.Noise,
            ExpirationEvent = GetExpirationEvent(statusRecord)
        };
    }

    private static ActiveComponentEffect? GetStatusRecord(GameEntity abilityEntity)
    {
        if (abilityEntity.Ability is not { Type: var abilityType }
            || !abilityType.IsStatusLike()
            || abilityEntity.Ability.OwnerEntity?.ActiveEffects is not { } activeEffects)
        {
            return null;
        }

        foreach (var record in activeEffects.ComponentEffects.GetValues(abilityEntity.Ability.Name!))
        {
            if (record.EffectType == EffectType.AddAbility
                && record.CarrierId == abilityEntity.Id)
            {
                return record;
            }
        }

        return null;
    }

    private static int? GetExpirationEvent(ActiveEffect? effect)
        => (int?)effect?.ExpirationEvent;

    private static int GetLevelReduction(GameEntity abilityEntity)
    {
        var ability = abilityEntity.Ability!;
        return ability.OwnerEntity is { } owner
            ? EffectApplicationSystem.GetAbilityLevelReduction(owner, ability.Name!)
            : 0;
    }

    public static int? GetAccumulator(GameEntity abilityEntity)
    {
        var ability = abilityEntity.Ability!;
        if (!ability.Type.IsStatusLike()
            || ability.OwnerEntity?.ActiveEffects is not { } activeEffects)
        {
            return null;
        }

        if (Effect.FindMetadata(ability.Name!) is not { } metadata
            || !metadata.CombinationFunction.IsBuildUp())
        {
            return null;
        }

        return activeEffects.Accumulators.GetValueOrDefault(metadata.EffectType)?.Value ?? 0;
    }
}
