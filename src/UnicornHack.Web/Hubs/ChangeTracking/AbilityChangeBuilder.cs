using UnicornHack.Generation.Effects;
using UnicornHack.Services;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs.ChangeTracking;

public class AbilityChangeBuilder
    : IEntityChangeListener<GameEntity>, IDependentEntityChangeListener<GameEntity>
{
    private const uint NameDirty = 1u << 1;
    private const uint ActivationDirty = 1u << 2;
    private const uint SlotDirty = 1u << 3;
    private const uint CooldownTickDirty = 1u << 4;
    private const uint CooldownXpLeftDirty = 1u << 5;
    private const uint TargetingShapeDirty = 1u << 6;
    private const uint TargetingShapeSizeDirty = 1u << 7;
    private const uint TypeDirty = 1u << 8;
    private const uint AccumulatorDirty = 1u << 9;
    private const uint LevelDirty = 1u << 10;
    private const uint LevelReductionDirty = 1u << 11;
    private const uint ExpirationTargetDirty = 1u << 12;
    private const uint ExpirationEventDirty = 1u << 13;
    private const uint NoiseDirty = 1u << 14;
    private const uint ExpirationDirty = ExpirationTargetDirty | ExpirationEventDirty;
    private const uint MembershipDirty = 1u << 31;
    private const uint AllValuesDirty = (1u << AbilityChange.PropertyCount) - 2;

    private readonly record struct PendingChange(GameEntity Entity, uint DirtyMask);

    private readonly record struct Projection(
        string? Name,
        int Activation,
        int? Slot,
        int? CooldownTick,
        int? CooldownXpLeft,
        int TargetingShape,
        int TargetingShapeSize,
        int Type,
        int? Accumulator,
        int Level,
        int LevelReduction,
        int? ExpirationTarget,
        int? ExpirationEvent,
        int? Noise);

    private readonly int _playerEntityId;
    private readonly Dictionary<int, PendingChange> _pending = [];
    private readonly Dictionary<int, Projection> _projection = [];
    private readonly Dictionary<int, Projection> _prepared = [];
    private readonly Dictionary<int, int> _accumulatorPlaceholders = [];
    private readonly HashSet<EffectType> _dirtyAccumulatorTypes = [];

    public AbilityChangeBuilder(int playerEntityId)
    {
        _playerEntityId = playerEntityId;
    }

    public bool HasAccumulatorChanges => _dirtyAccumulatorTypes.Count != 0;

    public bool HasChanges => _pending.Count != 0;

    public void RegisterOnGroups(GameManager manager)
    {
        manager.Abilities.AddListener(this);
        manager.AffectableEntities.AddListener(this);
        manager.AbilitiesToAffectableRelationship.AddDependentsListener(this);
    }

    public void UnregisterFromGroups(GameManager manager)
    {
        manager.Abilities.RemoveListener(this);
        manager.AffectableEntities.RemoveListener(this);
        manager.AbilitiesToAffectableRelationship.RemoveDependentsListener(this);
    }

    private bool IsRelevantPrincipal(GameEntity principal)
        => principal.Id == _playerEntityId;

    // Client-visible abilities have a slot or are slot-less status-like/race carriers. Status-like abilities
    // drive the HUD; races are included so generic ability transport can replace the dedicated race DTO.
    private bool IsRelevant(GameEntity entity)
        => entity.Ability is { } ability
            && ability.OwnerId == _playerEntityId
            && !ability.IsLeveledSupplement
            && IsClientVisible(ability);

    private bool IsRelevantOnRemove(in EntityChange<GameEntity> entityChange)
        => entityChange.RemovedComponent is AbilityComponent ability
            && ability.OwnerId == _playerEntityId
            && !ability.IsLeveledSupplement
            && IsClientVisible(ability);

    /// <summary>
    ///     Property-change handling must also detect <c>Slot</c> transitions, because slot
    ///     presence is what determines client visibility — the relationship events only
    ///     fire on ownership changes.
    /// </summary>
    private void OnBaseGroupPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
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

        MarkCandidate(entity, GetAbilityDirtyMask(entityChange.PropertyChanges));
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
        if (record?.TargetName is { } targetName
            && Effect.FindMetadata(targetName) is { } metadata)
        {
            _dirtyAccumulatorTypes.Add(metadata.EffectType);
        }

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

        MarkCandidate(abilityEntity, ExpirationDirty);
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
        MarkCandidate(abilityEntity, LevelReductionDirty);
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
        _dirtyAccumulatorTypes.Add(effectType);
        var metadata = Effect.GetMetadata(effectType);
        if (metadata.StatusAbilityName is not { } statusAbilityName
            || ownerEntity.Manager.AffectableAbilitiesIndex[
                (_playerEntityId, statusAbilityName)] is not { } abilityEntity)
        {
            return;
        }

        MarkCandidate(abilityEntity, AccumulatorDirty);
    }

    private void WriteAccumulatorPlaceholdersTo(
        Dictionary<int, AbilityChange> output, GameEntity ownerEntity)
    {
        foreach (var effectType in _dirtyAccumulatorTypes)
        {
            var id = GetAccumulatorPlaceholderId(effectType);
            var currentValue = GetAccumulatorPlaceholderValue(ownerEntity, effectType);
            if (currentValue is { } value)
            {
                if (!_accumulatorPlaceholders.ContainsKey(id))
                {
                    output.Add(id, CreateAccumulatorPlaceholder(effectType, value));
                }
                else
                {
                    output.Add(id, new AbilityChange { Accumulator = value });
                }

                _accumulatorPlaceholders[id] = value;
            }
            else if (_accumulatorPlaceholders.Remove(id))
            {
                output.Add(
                    id,
                    new AbilityChange { ChangedProperties = 0 });
            }
        }
    }

    public void ResetAccumulatorPlaceholders(GameEntity ownerEntity)
    {
        _accumulatorPlaceholders.Clear();
        foreach (var accumulator in ownerEntity.ActiveEffects!.Accumulators)
        {
            if (GetAccumulatorPlaceholderValue(ownerEntity, accumulator.EffectType) is { } value)
            {
                _accumulatorPlaceholders.Add(
                    GetAccumulatorPlaceholderId(accumulator.EffectType), value);
            }
        }

        _dirtyAccumulatorTypes.Clear();
    }

    public void ResetAccumulatorChanges() => _dirtyAccumulatorTypes.Clear();

    public static Dictionary<int, AbilityChange> GetAccumulatorPlaceholders(GameEntity ownerEntity)
    {
        var placeholderCount = 0;
        foreach (var accumulator in ownerEntity.ActiveEffects!.Accumulators)
        {
            var metadata = Effect.GetMetadata(accumulator.EffectType);
            if (metadata.StatusAbilityName is { } abilityName
                && ownerEntity.Manager.AffectableAbilitiesIndex[(ownerEntity.Id, abilityName)] == null)
            {
                placeholderCount++;
            }
        }

        var placeholders = new Dictionary<int, AbilityChange>(placeholderCount);
        foreach (var accumulator in ownerEntity.ActiveEffects!.Accumulators)
        {
            var metadata = Effect.GetMetadata(accumulator.EffectType);
            if (metadata.StatusAbilityName is not { } abilityName
                || ownerEntity.Manager.AffectableAbilitiesIndex[(ownerEntity.Id, abilityName)] != null)
            {
                continue;
            }

            var id = GetAccumulatorPlaceholderId(accumulator.EffectType);
            placeholders.Add(id, CreateAccumulatorPlaceholder(accumulator.EffectType, accumulator.Value));
        }

        return placeholders;
    }

    public static int GetAccumulatorPlaceholderId(EffectType effectType) => -1 - (int)effectType;

    public static string GetAccumulatorPlaceholderName(EffectType effectType)
        => Capitalize(Effect.GetMetadata(effectType).StatusAbilityName!);

    private static string Capitalize(string value)
        => value.Length == 0 ? value : char.ToUpperInvariant(value[0]) + value[1..];

    private static int? GetAccumulatorPlaceholderValue(GameEntity ownerEntity, EffectType effectType)
    {
        var metadata = Effect.GetMetadata(effectType);
        if (metadata.StatusAbilityName is not { } abilityName
            || ownerEntity.Manager.AffectableAbilitiesIndex[(ownerEntity.Id, abilityName)] != null)
        {
            return null;
        }

        return ownerEntity.ActiveEffects!.Accumulators.GetValueOrDefault(effectType)?.Value;
    }

    private static AbilityChange CreateAccumulatorPlaceholder(EffectType effectType, int value)
        => new()
        {
            ChangedProperties = null,
            Name = GetAccumulatorPlaceholderName(effectType),
            Activation = (int)ActivationType.Always,
            Type = (int)AbilityType.Debuff,
            Accumulator = value,
            Level = 0
        };

    public Dictionary<int, AbilityChange>? BuildChanges(
        in SerializationContext context, GameEntity ownerEntity)
    {
        var outputCount = CountAbilityChanges(context.Services)
                          + CountAccumulatorPlaceholderChanges(ownerEntity);
        if (outputCount == 0)
        {
            return null;
        }

        var output = new Dictionary<int, AbilityChange>(outputCount);
        foreach (var (id, pending) in _pending)
        {
            var hasPrevious = _projection.TryGetValue(id, out var previous);
            if (!TryGetProjection(
                    pending.Entity,
                    context.Services,
                    pending.DirtyMask,
                    hasPrevious ? previous : null,
                    out var current))
            {
                if (_projection.Remove(id))
                {
                    output.Add(
                        id,
                        new AbilityChange { ChangedProperties = 0 });
                }

                continue;
            }

            var change = hasPrevious
                ? SerializeDelta(current, pending.DirtyMask)
                : SerializeFull(current);

            _projection[id] = current;
            if (change != null)
            {
                output.Add(id, change);
            }
        }

        WriteAccumulatorPlaceholdersTo(output, ownerEntity);
        return output;
    }

    private int CountAbilityChanges(GameServices services)
    {
        var count = 0;
        foreach (var (id, pending) in _pending)
        {
            var hasPrevious = _projection.TryGetValue(id, out var previous);
            if (!TryGetProjection(
                    pending.Entity,
                    services,
                    pending.DirtyMask,
                    hasPrevious ? previous : null,
                    out var current))
            {
                if (_projection.ContainsKey(id))
                {
                    count++;
                }

                continue;
            }

            count++;
        }

        return count;
    }

    private int CountAccumulatorPlaceholderChanges(GameEntity ownerEntity)
    {
        var count = 0;
        foreach (var effectType in _dirtyAccumulatorTypes)
        {
            var id = GetAccumulatorPlaceholderId(effectType);
            var currentValue = GetAccumulatorPlaceholderValue(ownerEntity, effectType);
            if (currentValue != null)
            {
                count++;
            }
            else if (_accumulatorPlaceholders.ContainsKey(id))
            {
                count++;
            }
        }

        return count;
    }

    public void Reset()
    {
        _pending.Clear();
        _prepared.Clear();
    }

    public void ResetProjection(GameEntity ownerEntity)
    {
        _projection.Clear();
        _prepared.Clear();
        foreach (var abilityEntity in ownerEntity.Being!.Abilities)
        {
            if (TryGetProjection(
                    abilityEntity,
                    ownerEntity.Game.Services,
                    MembershipDirty | AllValuesDirty,
                    previous: null,
                    out var projection))
            {
                _projection[abilityEntity.Id] = projection;
            }
        }

        _prepared.Clear();
    }

    void IEntityChangeListener<GameEntity>.OnEntityAdded(in EntityChange<GameEntity> entityChange)
    {
        if (IsRelevant(entityChange.Entity))
        {
            MarkCandidate(entityChange.Entity, MembershipDirty | AllValuesDirty);
        }
    }

    void IEntityChangeListener<GameEntity>.OnEntityRemoved(in EntityChange<GameEntity> entityChange)
    {
        if (_projection.ContainsKey(entityChange.Entity.Id)
            || IsRelevantOnRemove(entityChange))
        {
            MarkCandidate(entityChange.Entity, MembershipDirty | AllValuesDirty);
        }
    }

    void IEntityChangeListener<GameEntity>.OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
        => OnBaseGroupPropertyValuesChanged(entityChange);

    void IDependentEntityChangeListener<GameEntity>.OnEntityAdded(
        in EntityChange<GameEntity> entityChange, GameEntity principal)
    {
        if (IsRelevantPrincipal(principal))
        {
            MarkCandidate(entityChange.Entity, MembershipDirty | AllValuesDirty);
        }
    }

    void IDependentEntityChangeListener<GameEntity>.OnEntityRemoved(
        in EntityChange<GameEntity> entityChange, GameEntity principal)
    {
        if (IsRelevantPrincipal(principal)
            && (_projection.ContainsKey(entityChange.Entity.Id) || IsRelevantOnRemove(entityChange)))
        {
            MarkCandidate(entityChange.Entity, MembershipDirty | AllValuesDirty);
        }
    }

    void IDependentEntityChangeListener<GameEntity>.OnPropertyValuesChanged(
        in EntityChange<GameEntity> entityChange, GameEntity principal)
    {
        if (IsRelevantPrincipal(principal))
        {
            MarkCandidate(entityChange.Entity, GetAbilityDirtyMask(entityChange.PropertyChanges));
        }
    }

    private void MarkCandidate(GameEntity entity, uint dirtyMask)
    {
        if (dirtyMask == 0)
        {
            return;
        }

        _pending[entity.Id] = _pending.TryGetValue(entity.Id, out var pending)
            ? pending with { Entity = entity, DirtyMask = pending.DirtyMask | dirtyMask }
            : new PendingChange(entity, dirtyMask);
    }

    private bool TryGetProjection(
        GameEntity entity,
        GameServices services,
        uint dirtyMask,
        Projection? previous,
        out Projection projection)
    {
        if (_prepared.TryGetValue(entity.Id, out projection))
        {
            return true;
        }

        if (entity.Ability is not { } ability
            || ability.OwnerId != _playerEntityId
            || ability.IsLeveledSupplement
            || !IsClientVisible(ability))
        {
            projection = default;
            return false;
        }

        var refreshAll = previous == null || (dirtyMask & MembershipDirty) != 0;
        var prior = previous.GetValueOrDefault();
        var statusRecord = refreshAll || (dirtyMask & ExpirationDirty) != 0
            ? GetStatusRecord(entity)
            : null;
        projection = new Projection(
            refreshAll || (dirtyMask & NameDirty) != 0
                ? ability.Type == AbilityType.Race
                ? services.Language.GetTemplateName(ability, abbreviate: false)
                : services.Language.GetString(ability)
                : prior.Name,
            refreshAll || (dirtyMask & ActivationDirty) != 0 ? (int)ability.Activation : prior.Activation,
            refreshAll || (dirtyMask & SlotDirty) != 0 ? ability.Slot : prior.Slot,
            refreshAll || (dirtyMask & CooldownTickDirty) != 0 ? ability.CooldownTick : prior.CooldownTick,
            refreshAll || (dirtyMask & CooldownXpLeftDirty) != 0 ? ability.CooldownXpLeft : prior.CooldownXpLeft,
            refreshAll || (dirtyMask & TargetingShapeDirty) != 0
                ? (int)ability.TargetingShape
                : prior.TargetingShape,
            refreshAll || (dirtyMask & TargetingShapeSizeDirty) != 0
                ? ability.TargetingShapeSize
                : prior.TargetingShapeSize,
            refreshAll || (dirtyMask & TypeDirty) != 0 ? (int)ability.Type : prior.Type,
            refreshAll || (dirtyMask & AccumulatorDirty) != 0 ? GetAccumulator(entity) : prior.Accumulator,
            refreshAll || (dirtyMask & LevelDirty) != 0 ? ability.Level : prior.Level,
            refreshAll || (dirtyMask & LevelReductionDirty) != 0
                ? GetLevelReduction(entity)
                : prior.LevelReduction,
            refreshAll || (dirtyMask & ExpirationTargetDirty) != 0
                ? statusRecord?.ExpirationTarget
                : prior.ExpirationTarget,
            refreshAll || (dirtyMask & ExpirationEventDirty) != 0
                ? GetExpirationEvent(statusRecord)
                : prior.ExpirationEvent,
            refreshAll || (dirtyMask & NoiseDirty) != 0 ? ability.Noise : prior.Noise);
        _prepared[entity.Id] = projection;
        return true;
    }

    private static uint GetAbilityDirtyMask(IPropertyValueChanges changes)
    {
        var dirtyMask = 0u;
        for (var i = 0; i < changes.Count; i++)
        {
            if (changes.GetChangedComponent(i).ComponentId != (int)EntityComponent.Ability)
            {
                continue;
            }

            dirtyMask |= changes.GetChangedPropertyName(i) switch
            {
                nameof(AbilityComponent.Name) => NameDirty | AccumulatorDirty | LevelReductionDirty
                                                  | ExpirationDirty | MembershipDirty,
                nameof(AbilityComponent.Activation) => ActivationDirty,
                nameof(AbilityComponent.Slot) => SlotDirty | MembershipDirty,
                nameof(AbilityComponent.CooldownTick) => CooldownTickDirty,
                nameof(AbilityComponent.CooldownXpLeft) => CooldownXpLeftDirty,
                nameof(AbilityComponent.TargetingShape) => TargetingShapeDirty,
                nameof(AbilityComponent.TargetingShapeSize) => TargetingShapeSizeDirty,
                nameof(AbilityComponent.Type) => NameDirty | TypeDirty | AccumulatorDirty
                                                  | ExpirationDirty | MembershipDirty,
                nameof(AbilityComponent.Level) => LevelDirty,
                nameof(AbilityComponent.Noise) => NoiseDirty,
                nameof(AbilityComponent.Action) => NameDirty,
                nameof(AbilityComponent.OwnerId) => MembershipDirty | AllValuesDirty,
                _ => 0
            };
        }

        return dirtyMask;
    }

    private static AbilityChange SerializeDelta(Projection current, uint dirtyMask)
    {
        var change = new AbilityChange();
        if ((dirtyMask & NameDirty) != 0) change.Name = current.Name;
        if ((dirtyMask & ActivationDirty) != 0) change.Activation = current.Activation;
        if ((dirtyMask & SlotDirty) != 0) change.Slot = current.Slot;
        if ((dirtyMask & CooldownTickDirty) != 0) change.CooldownTick = current.CooldownTick;
        if ((dirtyMask & CooldownXpLeftDirty) != 0) change.CooldownXpLeft = current.CooldownXpLeft;
        if ((dirtyMask & TargetingShapeDirty) != 0) change.TargetingShape = current.TargetingShape;
        if ((dirtyMask & TargetingShapeSizeDirty) != 0) change.TargetingShapeSize = current.TargetingShapeSize;
        if ((dirtyMask & TypeDirty) != 0) change.Type = current.Type;
        if ((dirtyMask & AccumulatorDirty) != 0) change.Accumulator = current.Accumulator;
        if ((dirtyMask & LevelDirty) != 0) change.Level = current.Level;
        if ((dirtyMask & LevelReductionDirty) != 0) change.LevelReduction = current.LevelReduction;
        if ((dirtyMask & ExpirationTargetDirty) != 0) change.ExpirationTarget = current.ExpirationTarget;
        if ((dirtyMask & ExpirationEventDirty) != 0) change.ExpirationEvent = current.ExpirationEvent;
        if ((dirtyMask & NoiseDirty) != 0) change.Noise = current.Noise;
        return change;
    }

    private static AbilityChange SerializeFull(Projection projection)
        => new()
        {
            ChangedProperties = null,
            Name = projection.Name,
            Activation = projection.Activation,
            Slot = projection.Slot,
            CooldownTick = projection.CooldownTick,
            CooldownXpLeft = projection.CooldownXpLeft,
            TargetingShape = projection.TargetingShape,
            TargetingShapeSize = projection.TargetingShapeSize,
            Type = projection.Type,
            Accumulator = projection.Accumulator,
            Level = projection.Level,
            LevelReduction = projection.LevelReduction,
            ExpirationTarget = projection.ExpirationTarget,
            ExpirationEvent = projection.ExpirationEvent,
            Noise = projection.Noise
        };

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
