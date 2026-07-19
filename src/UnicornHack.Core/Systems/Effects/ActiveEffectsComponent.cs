using System.Collections.Specialized;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Effects;

/// <summary>
///     Present on every affectable entity (exactly one per entity). Owns the runtime effect stores
///     (lingering modifier instances and structural records) plus the transient resolved-value cache
///     for that entity's dynamic properties. The resolved cache is rebuilt on load.
/// </summary>
[Component(Id = (int)EntityComponent.ActiveEffects)]
public class ActiveEffectsComponent : GameComponent
{
    private int? _nextEffectTick;
    private int _nextPropertyEffectId;
    private int _nextComponentEffectId;

    public ActiveEffectsComponent()
    {
        ComponentId = (int)EntityComponent.ActiveEffects;
        PropertyEffects = new ActivePropertyEffectCollection(this, nameof(PropertyEffects));
        ComponentEffects = new ActiveComponentEffectCollection(this, nameof(ComponentEffects));
        Accumulators = new EffectAccumulatorCollection(this, nameof(Accumulators));
        PropertyEffects.CollectionChanged += OnPropertyEffectsChanged;
        ComponentEffects.CollectionChanged += OnComponentEffectsChanged;
        Accumulators.CollectionChanged += OnAccumulatorsChanged;
    }

    private void OnPropertyEffectsChanged(object? sender, NotifyCollectionChangedEventArgs args)
        => OnCollectionChanged<ActivePropertyEffect>(args, nameof(PropertyEffects));

    private void OnComponentEffectsChanged(object? sender, NotifyCollectionChangedEventArgs args)
        => OnCollectionChanged<ActiveComponentEffect>(args, nameof(ComponentEffects));

    private void OnAccumulatorsChanged(object? sender, NotifyCollectionChangedEventArgs args)
        => OnCollectionChanged<EffectAccumulator>(args, nameof(Accumulators));

    /// <summary>
    ///     Earliest due modifier tick/expiry for this owner. This is an indexed temporal key, so it is
    ///     set through <see cref="Component.SetWithNotify{T}" /> to drive reindexing.
    /// </summary>
    public int? NextEffectTick
    {
        get => _nextEffectTick;
        set => SetWithNotify(value, ref _nextEffectTick);
    }

    /// <summary>The next persisted identifier for a property effect owned by this component.</summary>
    public int NextPropertyEffectId
    {
        get => _nextPropertyEffectId;
        private set => SetWithNotify(value, ref _nextPropertyEffectId);
    }

    /// <summary>The next persisted identifier for a component effect owned by this component.</summary>
    public int NextComponentEffectId
    {
        get => _nextComponentEffectId;
        private set => SetWithNotify(value, ref _nextComponentEffectId);
    }

    /// <summary>The lingering modifier effects active on this entity.</summary>
    public ActivePropertyEffectCollection PropertyEffects
    {
        get;
        private set;
    }

    /// <summary>The structural effects (added components) applied to this entity.</summary>
    public ActiveComponentEffectCollection ComponentEffects
    {
        get;
        private set;
    }

    public EffectAccumulatorCollection Accumulators
    {
        get;
        private set;
    }

    public ActivePropertyEffect AddPropertyEffect(
        EffectType effectType,
        string? targetName = null,
        ValueCombinationFunction combinationFunction = ValueCombinationFunction.Default,
        int? sourceAbilityEntityId = null,
        AbilityType sourceAbilityType = AbilityType.Default,
        ActivationType sourceAbilityActivation = ActivationType.Default,
        int magnitudePrimary = 0,
        int magnitudeSecondary = 0,
        int? expirationTarget = null,
        EffectDuration? expirationEvent = null)
    {
        var nextId = Math.Max(
            NextPropertyEffectId,
            GetNextInstanceId(PropertyEffects, static instance => instance.InstanceId));
        var effect = new ActivePropertyEffect(
            GameId, EntityId, nextId, effectType, targetName)
        {
            CombinationFunction = combinationFunction,
            SourceAbilityEntityId = sourceAbilityEntityId,
            SourceAbilityType = sourceAbilityType,
            SourceAbilityActivation = sourceAbilityActivation,
            MagnitudePrimary = magnitudePrimary,
            MagnitudeSecondary = magnitudeSecondary,
            ExpirationTarget = expirationTarget,
            ExpirationEvent = expirationEvent
        };
        NextPropertyEffectId = checked(nextId + 1);
        PropertyEffects.Add(effect);
        return effect;
    }

    public ActiveComponentEffect AddComponentEffect(
        EffectType effectType,
        string? targetName,
        int carrierId = 0,
        int sourceAbilityEntityId = 0,
        int? conditionValue = null,
        int level = 1,
        int? expirationTarget = null,
        EffectDuration? expirationEvent = null)
    {
        var nextId = Math.Max(
            NextComponentEffectId,
            GetNextInstanceId(ComponentEffects, static instance => instance.InstanceId));
        var effect = new ActiveComponentEffect(
            GameId, EntityId, nextId, effectType, targetName)
        {
            CarrierId = carrierId,
            SourceAbilityEntityId = sourceAbilityEntityId,
            ConditionValue = conditionValue,
            Level = level,
            ExpirationTarget = expirationTarget,
            ExpirationEvent = expirationEvent
        };
        NextComponentEffectId = checked(nextId + 1);
        ComponentEffects.Add(effect);
        return effect;
    }

    /// <summary>
    ///     Processes a mutable effect collection without allocating a snapshot. If processing removes or
    ///     repositions the current effect, iteration resumes at its new successor. Indexed collections use
    ///     direct access; other collections use their enumeration order.
    /// </summary>
    public static void ForEachEffect<TEffect, TState>(
        IList<TEffect> effects, TState state, Action<TEffect, TState> process)
        where TEffect : class
    {
        for (var effectIndex = 0; effectIndex < effects.Count;)
        {
            var effect = effects[effectIndex];
            process(effect, state);

            if (effectIndex < effects.Count
                && ReferenceEquals(effects[effectIndex], effect))
            {
                effectIndex++;
                continue;
            }

            var currentIndex = effects.IndexOf(effect);
            effectIndex = currentIndex >= 0
                ? currentIndex + 1
                : Math.Min(effectIndex, effects.Count);
        }
    }

    private static int GetNextInstanceId<T>(IEnumerable<T> effects, Func<T, int> getId)
    {
        var nextId = 0;
        foreach (var effect in effects)
        {
            var id = getId(effect);
            if (id >= nextId)
            {
                nextId = checked(id + 1);
            }
        }

        return nextId;
    }

    /// <summary>
    ///     The ability entities owned by this affectable entity (populated by the abilities relationship).
    ///     Transient: rebuilt from the owned abilities on load, never persisted.
    /// </summary>
    public IReadOnlyList<GameEntity> Abilities
    {
        get;
        private set;
    } = new List<GameEntity>();

    // Transient dynamic-property state (rebuilt on load, never persisted).
    private Dictionary<string, int>? _resolvedCache;
    private List<ActivePropertyEffect>? _contributionScratch;

    // Transient dynamic-magnitude dependency graph (rebuilt as dynamic instances register; never
    // persisted). Maps each input property to the instances whose cached magnitude tracks it, so a change
    // to that input can push a recompute onto exactly the dependent instances (the effect system drives
    // the cascade). Both maps stay null in the common case where no active instance has a stat-scaled
    // (dynamic) magnitude, so the graph adds no cost until something registers.
    private Dictionary<ActivePropertyEffect, DynamicMagnitude>? _dynamicMagnitudes;
    private Dictionary<string, List<ActivePropertyEffect>>? _magnitudeDependents;

    private sealed class DynamicMagnitude(string[] inputProperties, string outputProperty, Func<int> recompute)
    {
        public string[] InputProperties { get; } = inputProperties;
        public string OutputProperty { get; } = outputProperty;
        public Func<int> Recompute { get; } = recompute;
    }

    /// <summary>
    ///     Resolves an integer dynamic property by folding every modifier instance onto the property's
    ///     default in deterministic order, clamped to the property's range.
    /// </summary>
    public int ResolveInt(string property)
    {
        if (_resolvedCache != null
            && _resolvedCache.TryGetValue(property, out var cached))
        {
            return cached;
        }

        var result = Compute(property);
        (_resolvedCache ??= [])[property] = result;
        return result;
    }

    /// <summary>
    ///     Resolves a boolean dynamic property (e.g. <c>Infravision</c>): the base flag with every
    ///     contributing modifier folded on top using the boolean combination rules.
    /// </summary>
    public bool ResolveBool(string property)
    {
        if (_resolvedCache != null
            && _resolvedCache.TryGetValue(property, out var cachedBool))
        {
            return cachedBool != 0;
        }

        var count = GatherSortedContributions(property);

        var descriptor = Entity.Manager.Game.Services.Data.PropertyDescriptions.Find(property)
            as PropertyDescription<bool>;
        var value = descriptor?.DefaultValue ?? false;

        if (count != 0)
        {
            var contributions = _contributionScratch!;
            var runningSum = value ? 1 : 0;
            var summandCount = 0;
            var buildUpSum = 0;
            var hasBuildUp = false;
            var buildUpFunction = contributions[0].CombinationFunction;
            for (var i = 0; i < count; i++)
            {
                var contribution = contributions[i];
                if (contribution.CombinationFunction.IsBuildUp())
                {
                    // Threshold accumulators bank their raw magnitude; they take hold together once the
                    // banked total covers their threshold, so fold them separately from the booleans. All
                    // contributions to one condition share a variant, so the last one's threshold applies.
                    Debug.Assert(!hasBuildUp || contribution.CombinationFunction == buildUpFunction,
                        $"Build-up contributions to '{property}' mix combination functions "
                        + $"{buildUpFunction} and {contribution.CombinationFunction}.");
                    buildUpSum += contribution.MagnitudePrimary;
                    hasBuildUp = true;
                    buildUpFunction = contribution.CombinationFunction;
                    continue;
                }

                PropertyValueCombiner.ApplyBool(
                    contribution.MagnitudePrimary != 0, contribution.CombinationFunction,
                    ref value, ref runningSum, ref summandCount);
            }

            if (hasBuildUp)
            {
                PropertyValueCombiner.ApplyBool(
                    buildUpSum >= EffectApplicationSystem.GetBuildUpThreshold(buildUpFunction, Entity),
                    ValueCombinationFunction.Sum, ref value, ref runningSum, ref summandCount);
            }

            contributions.Clear();
        }

        (_resolvedCache ??= [])[property] = value ? 1 : 0;
        return value;
    }

    /// <summary>
    ///     Recomputes a resolved property, compares it against the last value observed by subscribers
    ///     (mirroring the legacy backing field, which starts at <c>default</c>), records the new value,
    ///     and reports whether it changed. Used by the effect engine to raise change notifications for
    ///     resolved properties without an owning backing field.
    /// </summary>
    public bool TryRefresh(string property, bool isBool, out int oldValue, out int newValue)
    {
        var cache = _resolvedCache ??= [];
        if (!cache.TryGetValue(property, out oldValue))
        {
            oldValue = 0;
        }

        cache.Remove(property);
        newValue = isBool ? (ResolveBool(property) ? 1 : 0) : ResolveInt(property);

        return oldValue != newValue;
    }

    /// <summary>
    ///     Registers a dynamic-magnitude dependency: <paramref name="instance" />'s
    ///     <see cref="ActivePropertyEffect.MagnitudePrimary" /> is recomputed by <paramref name="recompute" />
    ///     whenever any property in <paramref name="inputProperties" /> changes on this entity, and the new
    ///     magnitude feeds the instance's own <see cref="ActivePropertyEffect.TargetName" />. A binding
    ///     may not read the property it feeds, so a self-referential binding is rejected up front. Registering
    ///     the same instance again replaces its previous binding. The graph is transient (never persisted); a
    ///     consumer that adds dynamic instances is responsible for registering and unregistering them.
    /// </summary>
    public void RegisterDynamicMagnitude(
        ActivePropertyEffect instance, IReadOnlyList<string> inputProperties, Func<int> recompute)
    {
        var output = instance.TargetName
            ?? throw new InvalidOperationException("A dynamic magnitude requires a target property.");

        var inputs = new string[inputProperties.Count];
        for (var i = 0; i < inputProperties.Count; i++)
        {
            var input = inputProperties[i];
            if (input == output
                || HasDependencyPath(output, input, instance))
            {
                throw new InvalidOperationException(
                    $"Dynamic magnitude dependency '{input}' -> '{output}' creates a cycle.");
            }

            inputs[i] = input;
        }

        _dynamicMagnitudes ??= [];
        _magnitudeDependents ??= [];

        if (_dynamicMagnitudes.TryGetValue(instance, out var existing))
        {
            RemoveDependencyEdges(instance, existing.InputProperties);
        }

        _dynamicMagnitudes[instance] = new DynamicMagnitude(inputs, output, recompute);
        foreach (var input in inputs)
        {
            if (!_magnitudeDependents.TryGetValue(input, out var dependents))
            {
                dependents = [];
                _magnitudeDependents[input] = dependents;
            }

            if (!dependents.Contains(instance))
            {
                dependents.Add(instance);
            }
        }
    }

    private bool HasDependencyPath(
        string sourceProperty, string targetProperty, ActivePropertyEffect ignoredInstance)
    {
        if (_magnitudeDependents == null)
        {
            return false;
        }

        var pending = new Stack<string>();
        var visited = new HashSet<string>();
        pending.Push(sourceProperty);
        while (pending.TryPop(out var property))
        {
            if (!visited.Add(property)
                || !_magnitudeDependents.TryGetValue(property, out var dependents))
            {
                continue;
            }

            foreach (var dependent in dependents)
            {
                if (ReferenceEquals(dependent, ignoredInstance)
                    || _dynamicMagnitudes == null
                    || !_dynamicMagnitudes.TryGetValue(dependent, out var registration))
                {
                    continue;
                }

                if (registration.OutputProperty == targetProperty)
                {
                    return true;
                }

                pending.Push(registration.OutputProperty);
            }
        }

        return false;
    }

    internal void ClearDynamicMagnitudes()
    {
        _dynamicMagnitudes?.Clear();
        _magnitudeDependents?.Clear();
    }

    /// <summary>Removes an instance from the dynamic-magnitude graph (call when the instance is removed).</summary>
    public void UnregisterDynamicMagnitude(ActivePropertyEffect instance)
    {
        if (_dynamicMagnitudes == null
            || !_dynamicMagnitudes.TryGetValue(instance, out var registration))
        {
            return;
        }

        RemoveDependencyEdges(instance, registration.InputProperties);
        _dynamicMagnitudes.Remove(instance);
    }

    private void RemoveDependencyEdges(ActivePropertyEffect instance, string[] inputs)
    {
        if (_magnitudeDependents == null)
        {
            return;
        }

        foreach (var input in inputs)
        {
            if (_magnitudeDependents.TryGetValue(input, out var dependents))
            {
                dependents.Remove(instance);
                if (dependents.Count == 0)
                {
                    _magnitudeDependents.Remove(input);
                }
            }
        }
    }

    /// <summary>True if any active dynamic-magnitude instance reads the given property as one of its inputs.</summary>
    public bool HasDynamicMagnitudeDependents(string property)
        => _magnitudeDependents != null && _magnitudeDependents.ContainsKey(property);

    /// <summary>
    ///     Recomputes every dynamic-magnitude instance that reads <paramref name="changedProperty" /> and
    ///     appends the distinct output properties whose contribution actually changed to
    ///     <paramref name="changedOutputs" />, so the caller can re-resolve and cascade them. Only an instance
    ///     whose recomputed <see cref="ActivePropertyEffect.MagnitudePrimary" /> differs invalidates and
    ///     reports its output, which is what lets a cascade settle to a fixpoint.
    /// </summary>
    public void RecomputeDynamicMagnitudes(string changedProperty, List<string> changedOutputs)
    {
        if (_magnitudeDependents == null
            || !_magnitudeDependents.TryGetValue(changedProperty, out var dependents))
        {
            return;
        }

        for (var i = 0; i < dependents.Count; i++)
        {
            var instance = dependents[i];
            if (_dynamicMagnitudes == null
                || !_dynamicMagnitudes.TryGetValue(instance, out var registration))
            {
                continue;
            }

            var newMagnitude = registration.Recompute();
            if (newMagnitude == instance.MagnitudePrimary)
            {
                continue;
            }

            instance.MagnitudePrimary = newMagnitude;
            if (!changedOutputs.Contains(registration.OutputProperty))
            {
                changedOutputs.Add(registration.OutputProperty);
            }
        }
    }

    // Fills _contributionScratch with the (sorted) contributions targeting the given property and
    // returns their count. Shared by the int and bool resolvers. Every contribution is a modifier
    // instance: innate/derived base modifiers, timed and status modifiers, and the materialized mirrors
    // of lingering applied ChangeProperty effects (kept in sync by EffectApplicationSystem). Each
    // instance is self-describing: its TargetName/CombinationFunction declare which property it
    // contributes to and how. Source type establishes three layers: Innate base values, ordinary modifiers,
    // then Status conditions so an active status Override cannot be undone by an ordinary Sum.
    private int GatherSortedContributions(string property)
    {
        var contributions = _contributionScratch ??= [];
        contributions.Clear();

        var propertyEffects = PropertyEffects.GetValues(property);
        for (var i = 0; i < propertyEffects.Count; i++)
        {
            contributions.Add(propertyEffects[i]);
        }

        if (contributions.Count > 1)
        {
            contributions.Sort(static (x, y) =>
            {
                var byType = GetSourceLayer(x.SourceAbilityType) - GetSourceLayer(y.SourceAbilityType);
                if (byType != 0)
                {
                    return byType;
                }

                var byTier = PropertyValueCombiner.GetActivationOrder(x.SourceAbilityActivation)
                             - PropertyValueCombiner.GetActivationOrder(y.SourceAbilityActivation);
                if (byTier != 0)
                {
                    return byTier;
                }

                // Higher-precedence (Absolute: Override/Min/Max/Mean) functions apply before lower
                // (Relative: Percent/Sum). Ties break by stable id.
                var byFunction = (int)y.CombinationFunction - (int)x.CombinationFunction;
                return byFunction != 0 ? byFunction : x.InstanceId - y.InstanceId;
            });
        }

        return contributions.Count;
    }

    private static int GetSourceLayer(AbilityType abilityType)
        => abilityType == AbilityType.Innate
            ? 0
            : abilityType.IsStatusLike() ? 2 : 1;

    private int Compute(string property)
    {
        var descriptor = Entity.Manager.Game.Services.Data.PropertyDescriptions.Find(property)
            as PropertyDescription<int>;

        var baseValue = descriptor?.DefaultValue ?? 0;

        var count = GatherSortedContributions(property);

        var value = baseValue;
        if (count != 0)
        {
            var contributions = _contributionScratch!;
            var runningSum = value;
            var summandCount = 0;
            for (var i = 0; i < count; i++)
            {
                var contribution = contributions[i];
                PropertyValueCombiner.Apply(
                    contribution.MagnitudePrimary, contribution.CombinationFunction,
                    ref value, ref runningSum, ref summandCount);
            }

            contributions.Clear();
        }

        if (descriptor != null)
        {
            if (descriptor.MinValue is { } min && value < min)
            {
                value = min;
            }

            if (descriptor.MaxValue is { } max && value > max)
            {
                value = max;
            }
        }

        return value;
    }

    protected override void Clean()
    {
        _nextEffectTick = default;
        _nextPropertyEffectId = default;
        _nextComponentEffectId = default;
        PropertyEffects.Clear();
        ComponentEffects.Clear();
        Accumulators.Clear();
        ((List<GameEntity>)Abilities).Clear();
        _resolvedCache?.Clear();
        _contributionScratch?.Clear();
        _dynamicMagnitudes?.Clear();
        _magnitudeDependents?.Clear();

        base.Clean();
    }
}
