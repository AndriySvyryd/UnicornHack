using UnicornHack.Generation.Effects;
using UnicornHack.Systems.Effects;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs.ChangeTracking;

internal sealed class EffectChangeBuilder(int playerEntityId) : IEntityChangeListener<GameEntity>
{
    internal readonly record struct DamageOverTimeEffect(int ExpirationTick, int Magnitude);

    private readonly record struct DamageOverTimeContribution(
        EffectType EffectType,
        ValueCombinationFunction CombinationFunction,
        int ExpirationTick,
        int Magnitude);

    private readonly record struct PendingEffect(
        EffectType EffectType,
        DamageOverTimeEffect? Initial,
        DamageOverTimeEffect? Current);

    private readonly Dictionary<int, PendingEffect> _changes = [];

    public bool HasChanges => _changes.Count != 0;

    public void RegisterOnGroups(GameManager manager)
        => manager.AffectableEntities.AddListener(this);

    public void UnregisterFromGroups(GameManager manager)
        => manager.AffectableEntities.RemoveListener(this);

    public Dictionary<int, EffectChange>? BuildChanges()
    {
        if (_changes.Count == 0)
        {
            return null;
        }

        var output = new Dictionary<int, EffectChange>(_changes.Count);
        foreach (var (id, pending) in _changes)
        {
            if (pending.Initial == null)
            {
                output.Add(id, CreateSnapshot(pending.EffectType, pending.Current!.Value));
            }
            else if (pending.Current == null)
            {
                output.Add(id, new EffectChange { ChangedProperties = 0 });
            }
            else
            {
                output.Add(id, CreateDelta(pending.Initial.Value, pending.Current.Value));
            }
        }

        return output;
    }

    public void Reset() => _changes.Clear();

    public static Dictionary<int, EffectChange> SerializeSnapshot(GameEntity entity)
    {
        var current = new Dictionary<EffectType, DamageOverTimeEffect>();
        Collect(entity, current);

        var result = new Dictionary<int, EffectChange>(current.Count);
        foreach (var (effectType, values) in current)
        {
            result[GetKey(effectType)] = CreateSnapshot(effectType, values);
        }

        return result;
    }

    private static void Collect(
        GameEntity entity, Dictionary<EffectType, DamageOverTimeEffect> into)
    {
        into.Clear();
        if (entity.ActiveEffects is not { } activeEffects)
        {
            return;
        }

        foreach (var instance in activeEffects.PropertyEffects)
        {
            if (!IsDamageOverTime(instance.EffectType, instance.CombinationFunction))
            {
                continue;
            }

            var expirationTick = GetExpirationTick(instance.ExpirationEvent, instance.ExpirationTarget);
            into[instance.EffectType] = into.TryGetValue(instance.EffectType, out var existing)
                ? new DamageOverTimeEffect(
                    Math.Max(existing.ExpirationTick, expirationTick),
                    existing.Magnitude + instance.MagnitudePrimary)
                : new DamageOverTimeEffect(expirationTick, instance.MagnitudePrimary);
        }
    }

    private static DamageOverTimeEffect? GetCurrent(
        GameEntity entity,
        EffectType effectType,
        ActivePropertyEffect? excluded = null,
        DamageOverTimeContribution? included = null)
    {
        if (Effect.GetMetadata(effectType).ResourceProperty == null)
        {
            return null;
        }

        var hasValue = false;
        var expirationTick = 0;
        var magnitude = 0;
        foreach (var instance in entity.ActiveEffects!.PropertyEffects)
        {
            if (ReferenceEquals(instance, excluded)
                || instance.EffectType != effectType
                || instance.CombinationFunction != ValueCombinationFunction.Subtract)
            {
                continue;
            }

            hasValue = true;
            expirationTick = Math.Max(
                expirationTick,
                GetExpirationTick(instance.ExpirationEvent, instance.ExpirationTarget));
            magnitude += instance.MagnitudePrimary;
        }

        if (included is
            {
                EffectType: var includedType,
                CombinationFunction: ValueCombinationFunction.Subtract
            } contribution
            && includedType == effectType)
        {
            hasValue = true;
            expirationTick = Math.Max(expirationTick, contribution.ExpirationTick);
            magnitude += contribution.Magnitude;
        }

        return hasValue ? new DamageOverTimeEffect(expirationTick, magnitude) : null;
    }

    private void Track(
        GameEntity entity, EffectType effectType,
        DamageOverTimeEffect? oldValue, DamageOverTimeEffect? newValue)
    {
        if (oldValue == newValue)
        {
            return;
        }

        var key = GetKey(effectType);
        if (!_changes.TryGetValue(key, out var pending))
        {
            _changes[key] = new PendingEffect(effectType, oldValue, newValue);
            return;
        }

        pending = pending with { Current = newValue };
        if (pending.Initial == pending.Current)
        {
            _changes.Remove(key);
            return;
        }

        _changes[key] = pending;
    }

    private void TrackCollectionChange(
        GameEntity entity, IPropertyValueChanges changes, int changeIndex)
    {
        if (changes.GetNestedItem(changeIndex) is ActivePropertyEffect changed)
        {
            if (changes.GetNestedPropertyName(changeIndex) is not (
                    nameof(ActivePropertyEffect.EffectType)
                    or nameof(ActivePropertyEffect.CombinationFunction)
                    or nameof(ActiveEffect.ExpirationTarget)
                    or nameof(ActiveEffect.ExpirationEvent)
                    or nameof(ActivePropertyEffect.MagnitudePrimary)))
            {
                return;
            }

            var oldContribution = GetOldContribution(changed, changes, changeIndex);
            TrackChangedType(entity, changed, oldContribution.EffectType, oldContribution);
            if (changed.EffectType != oldContribution.EffectType)
            {
                TrackChangedType(entity, changed, changed.EffectType, oldContribution);
            }

            return;
        }

        var oldChange = changes.GetValue<EntityCollectionChange>(changeIndex, Utils.MessagingECS.ValueType.Old);
        var newChange = changes.GetValue<EntityCollectionChange>(changeIndex, Utils.MessagingECS.ValueType.Current);
        if (newChange is { Action: EntityCollectionChangeAction.Add, Item: ActivePropertyEffect added })
        {
            Track(
                entity, added.EffectType,
                GetCurrent(entity, added.EffectType, excluded: added),
                GetCurrent(entity, added.EffectType));
            return;
        }

        if (oldChange is { Action: EntityCollectionChangeAction.Remove, Item: ActivePropertyEffect removed })
        {
            Track(
                entity, removed.EffectType,
                GetCurrent(entity, removed.EffectType, included: GetContribution(removed)),
                GetCurrent(entity, removed.EffectType));
            return;
        }

    }

    private void TrackChangedType(
        GameEntity entity, ActivePropertyEffect changed, EffectType effectType,
        DamageOverTimeContribution oldContribution)
        => Track(
            entity, effectType,
            GetCurrent(entity, effectType, excluded: changed, included: oldContribution),
            GetCurrent(entity, effectType));

    private static DamageOverTimeContribution GetOldContribution(
        ActivePropertyEffect effect, IPropertyValueChanges changes, int changeIndex)
    {
        var changedProperty = changes.GetNestedPropertyName(changeIndex);
        var expirationEvent = changedProperty == nameof(ActiveEffect.ExpirationEvent)
            ? changes.GetValue<EffectDuration?>(changeIndex, Utils.MessagingECS.ValueType.Old)
            : effect.ExpirationEvent;
        var expirationTarget = changedProperty == nameof(ActiveEffect.ExpirationTarget)
            ? changes.GetValue<int?>(changeIndex, Utils.MessagingECS.ValueType.Old)
            : effect.ExpirationTarget;
        return new DamageOverTimeContribution(
            changedProperty == nameof(ActivePropertyEffect.EffectType)
                ? changes.GetValue<EffectType>(changeIndex, Utils.MessagingECS.ValueType.Old)
                : effect.EffectType,
            changedProperty == nameof(ActivePropertyEffect.CombinationFunction)
                ? changes.GetValue<ValueCombinationFunction>(changeIndex, Utils.MessagingECS.ValueType.Old)
                : effect.CombinationFunction,
            GetExpirationTick(expirationEvent, expirationTarget),
            changedProperty == nameof(ActivePropertyEffect.MagnitudePrimary)
                ? changes.GetValue<int>(changeIndex, Utils.MessagingECS.ValueType.Old)
                : effect.MagnitudePrimary);
    }

    private static DamageOverTimeContribution GetContribution(ActivePropertyEffect effect)
        => new(
            effect.EffectType,
            effect.CombinationFunction,
            GetExpirationTick(effect.ExpirationEvent, effect.ExpirationTarget),
            effect.MagnitudePrimary);

    private static int GetExpirationTick(EffectDuration? expirationEvent, int? expirationTarget)
        => expirationEvent == EffectDuration.UntilTimeout ? expirationTarget ?? 0 : 0;

    private static bool IsDamageOverTime(
        EffectType effectType, ValueCombinationFunction combinationFunction)
        => combinationFunction == ValueCombinationFunction.Subtract
           && Effect.GetMetadata(effectType).ResourceProperty != null;

    private static EffectChange CreateSnapshot(EffectType effectType, DamageOverTimeEffect values)
        => new()
        {
            ChangedProperties = null,
            EffectType = effectType,
            ExpirationTick = values.ExpirationTick,
            Magnitude = values.Magnitude,
            InForce = true
        };

    private static EffectChange CreateDelta(
        DamageOverTimeEffect oldValue, DamageOverTimeEffect newValue)
    {
        var change = new EffectChange();
        if (oldValue.ExpirationTick != newValue.ExpirationTick)
        {
            change.ExpirationTick = newValue.ExpirationTick;
        }

        if (oldValue.Magnitude != newValue.Magnitude)
        {
            change.Magnitude = newValue.Magnitude;
        }

        return change;
    }

    private static int GetKey(EffectType effectType) => -(int)effectType - 1;

    void IEntityChangeListener<GameEntity>.OnEntityAdded(in EntityChange<GameEntity> entityChange)
    {
    }

    void IEntityChangeListener<GameEntity>.OnEntityRemoved(in EntityChange<GameEntity> entityChange)
    {
    }

    void IEntityChangeListener<GameEntity>.OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
    {
        if (entityChange.Entity.Id != playerEntityId)
        {
            return;
        }

        var changes = entityChange.PropertyChanges;
        for (var i = 0; i < changes.Count; i++)
        {
            if (changes.GetChangedComponent(i).ComponentId != (int)EntityComponent.ActiveEffects
                || changes.GetChangedPropertyName(i) != nameof(ActiveEffectsComponent.PropertyEffects))
            {
                continue;
            }

            TrackCollectionChange(entityChange.Entity, changes, i);
        }
    }
}
