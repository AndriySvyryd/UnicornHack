using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnicornHack.Systems.Beings;

namespace UnicornHack.Systems.Effects;

// Evaluation of an effect's amount/duration expressions: compiling the authored expression strings into
// delegates, invoking them against the effect, activator and target, the Being-property variables and
// stat-modifier functions the expressions may use, and the dependency mapping that lets a stat-derived
// magnitude live-track its activator inputs.
public partial class EffectApplicationSystem
{
    private static readonly ParameterExpression EffectParameter =
        Expression.Parameter(typeof(GameEntity), name: "effect");

    private static readonly ParameterExpression ActivatorParameter =
        Expression.Parameter(typeof(GameEntity), name: "activator");

    private static readonly ParameterExpression TargetParameter =
        Expression.Parameter(typeof(GameEntity), name: "target");

    private static readonly ParameterExpression AbilityLevelParameter =
        Expression.Parameter(typeof(int), name: "level");

    private static readonly EffectExpressionVisitor _translator =
        new([EffectParameter, ActivatorParameter, TargetParameter]);

    private static readonly EffectExpressionVisitor _statusAbilityDurationTranslator =
        new([EffectParameter, ActivatorParameter, TargetParameter, AbilityLevelParameter]);

    private static readonly ConcurrentDictionary<string, Lazy<AmountExpression>> AmountExpressions =
        new(StringComparer.Ordinal);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    private static readonly Type BeingComponentType = typeof(BeingComponent);

    private static readonly PropertyInfo GameEntityBeingProperty = typeof(GameEntity)
        .GetProperty(nameof(GameEntity.Being))!;

    private static readonly IReadOnlyDictionary<string, PropertyInfo> BeingProperties =
        BeingComponentType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(property => property.GetMethod != null)
            .ToDictionary(property => property.Name, StringComparer.OrdinalIgnoreCase);

    private static readonly MethodInfo _requirementsModifierMethod = typeof(EffectApplicationSystem)
        .GetMethod(nameof(GetRequirementsModifier), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo _mightModifierMethod = typeof(EffectApplicationSystem)
        .GetMethod(nameof(GetMightModifier), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo _focusModifierMethod = typeof(EffectApplicationSystem)
        .GetMethod(nameof(GetFocusModifier), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo _perceptionModifierMethod = typeof(EffectApplicationSystem)
        .GetMethod(nameof(GetPerceptionModifier), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo _speedModifierMethod = typeof(EffectApplicationSystem)
        .GetMethod(nameof(GetSpeedModifier), BindingFlags.NonPublic | BindingFlags.Static)!;

    public int GetDurationAmount(
        EffectComponent effect, GameEntity activatorEntity, GameEntity? targetEntity)
    {
        if (effect.DurationAmount == null)
        {
            return 0;
        }

        var ability = effect.ContainingAbility!.Ability!;
        effect.DurationAmountFunction ??= CreateAmountFunction(effect.DurationAmount, ability.Name!, effect.EffectType.ToString());
        return InvokeAmountFunction(
            effect.DurationAmountFunction, effect.Entity, activatorEntity, targetEntity,
            "Duration", ability.Name!);
    }

    public int GetAmount(EffectComponent effect, GameEntity activatorEntity, GameEntity? targetEntity)
    {
        if (effect.Amount == null)
        {
            return 0;
        }

        var ability = effect.ContainingAbility!.Ability!;
        var amountFunction = effect.AmountFunction ??= CreateAmountFunction(effect.Amount, ability.Name!, effect.EffectType.ToString());
        return InvokeAmountFunction(
            amountFunction, effect.Entity, activatorEntity, targetEntity, "Amount", ability.Name!);
    }

    public int GetSecondaryAmount(
        EffectComponent effect, GameEntity activatorEntity, GameEntity? targetEntity)
    {
        if (effect.SecondaryAmount == null)
        {
            return 0;
        }

        var ability = effect.ContainingAbility!.Ability!;
        effect.SecondaryAmountFunction ??= CreateAmountFunction(effect.SecondaryAmount, ability.Name!, effect.EffectType.ToString());
        return InvokeAmountFunction(
            effect.SecondaryAmountFunction, effect.Entity, activatorEntity, targetEntity,
            "SecondaryAmount", ability.Name!);
    }

    public static Func<GameEntity, GameEntity, GameEntity?, float> CreateAmountFunction(
        string expression, string abilityName, string effectName)
    {
        if (expression == Effect.PropagatedAmountPlaceholder)
        {
            return static (_, _, _) => throw new InvalidOperationException(
                "A propagated amount placeholder cannot be evaluated.");
        }

        try
        {
            return GetAmountExpression(expression).Function.Value;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Error while parsing the Amount for {abilityName}, {effectName}", e);
        }
    }

    public static Func<GameEntity, GameEntity, GameEntity?, int, int?> CreateStatusAbilityDurationFunction(
        string expression, string abilityName, string effectName)
    {
        try
        {
            lock (_statusAbilityDurationTranslator)
            {
                return _statusAbilityDurationTranslator.Translate<
                    Func<GameEntity, GameEntity, GameEntity?, int, int?>, int?>(expression);
            }
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Error while parsing the status ability duration for {abilityName}, {effectName}", e);
        }
    }

    internal static int? InvokeStatusAbilityDurationFunction(
        Func<GameEntity, GameEntity, GameEntity?, int, int?> function,
        GameEntity targetEntity, int level, string abilityName)
    {
        try
        {
            return function(targetEntity, targetEntity, targetEntity, level);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Error while evaluating the status ability duration for {abilityName} level {level}", e);
        }
    }

    // Evaluates a compiled amount/duration expression, wrapping any evaluation error with the effect role
    // and owning ability for diagnostics.
    internal static int InvokeAmountFunction(
        Func<GameEntity, GameEntity, GameEntity?, float> function,
        GameEntity effectEntity, GameEntity activatorEntity, GameEntity? targetEntity,
        string role, string abilityName)
    {
        try
        {
            return (int)function(effectEntity, activatorEntity, targetEntity);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Error while evaluating the {role} for {abilityName}", e);
        }
    }

    private static float GetRequirementsModifier(GameEntity effectEntity, GameEntity activatorEntity)
    {
        var manager = effectEntity.Manager;
        var item = effectEntity.Effect!.ContainingAbility!.Ability!.OwnerEntity!.Item!;

        //TODO: Show stats for each equipable slot
        var slot = item.EquippedSlot == EquipmentSlot.None
            ? EquipmentSlot.GraspPrimaryMelee
            : item.EquippedSlot;
        var handnessMultiplier = manager.SkillAbilitiesSystem.GetHandnessMultiplier(slot);
        if (handnessMultiplier <= 0)
        {
            return 0;
        }

        var baseMultiplier =
            manager.SkillAbilitiesSystem.GetHandnessMultiplier(EquipmentSlot.GraspPrimaryMelee);
        var template = manager.Game.Services.Data.Items.Get(item.TemplateName);
        var skillBonus = manager.SkillAbilitiesSystem.GetItemSkillBonus(template, activatorEntity);

        var requiredMight = template.RequiredMight ?? 0;
        var mightDifference = activatorEntity.Being!.Might * handnessMultiplier +
                              (skillBonus - requiredMight) * baseMultiplier;
        var requiredFocus = template.RequiredFocus ?? 0;
        var focusDifference = activatorEntity.Being.Focus * handnessMultiplier +
                              (skillBonus - requiredFocus) * baseMultiplier;
        var scale = 100
                    + ((requiredMight * (mightDifference + Math.Min(0, mightDifference)))
                       + (requiredFocus * (focusDifference + Math.Min(0, focusDifference)))) /
                    handnessMultiplier;

        if (scale <= 0)
        {
            return 0;
        }

        return scale / 100f;
    }

    private static float GetMightModifier(GameEntity effectEntity, GameEntity activatorEntity)
        => (10 + activatorEntity.Being!.Might) / 10f;

    private static float GetFocusModifier(GameEntity effectEntity, GameEntity activatorEntity)
        => (10 + activatorEntity.Being!.Focus) / 10f;

    private static float GetPerceptionModifier(GameEntity effectEntity, GameEntity activatorEntity)
        => (10 + activatorEntity.Being!.Perception) / 10f;

    private static float GetSpeedModifier(GameEntity effectEntity, GameEntity activatorEntity)
        => (10 + activatorEntity.Being!.Speed) / 10f;

    // Maps each stat-modifier function usable in an amount expression to the resolved properties it reads,
    // so a dynamic magnitude derived from that expression can be recomputed when any of those properties
    // changes on the (self-targeted) owner. Kept in step with the functions the EffectExpressionVisitor
    // recognizes.
    private static readonly (MethodInfo Method, string[] Properties)[] AmountInputModifiers =
    [
        (_mightModifierMethod, [nameof(BeingComponent.Might)]),
        (_focusModifierMethod, [nameof(BeingComponent.Focus)]),
        (_perceptionModifierMethod, [nameof(BeingComponent.Perception)]),
        (_speedModifierMethod, [nameof(BeingComponent.Speed)]),
        (_requirementsModifierMethod, [nameof(BeingComponent.Might), nameof(BeingComponent.Focus)])
    ];

    // The distinct activator properties an amount expression reads through a bare variable or one of its
    // stat-modifier functions. Target-qualified variables are intentionally excluded because dynamic
    // magnitudes only track the self-targeted activator.
    public static IReadOnlyList<string> GetAmountInputProperties(string? expression)
    {
        if (string.IsNullOrEmpty(expression))
        {
            return Array.Empty<string>();
        }

        return GetAmountExpression(expression).InputProperties;
    }

    private static AmountExpression GetAmountExpression(string expression)
        => AmountExpressions.GetOrAdd(
            expression,
            static value => new Lazy<AmountExpression>(
                () => CreateAmountExpression(value), LazyThreadSafetyMode.ExecutionAndPublication)).Value;

    private static AmountExpression CreateAmountExpression(string expression)
    {
        Expression<Func<GameEntity, GameEntity, GameEntity?, float>> tree;
        lock (_translator)
        {
            tree = _translator.TranslateExpression<
                Func<GameEntity, GameEntity, GameEntity?, float>, float>(expression);
        }

        var inputVisitor = new AmountInputExpressionVisitor();
        inputVisitor.Visit(tree.Body);
        return new AmountExpression(
            tree,
            new Lazy<Func<GameEntity, GameEntity, GameEntity?, float>>(
                () => tree.Compile(preferInterpretation: !RuntimeFeature.IsDynamicCodeSupported),
                LazyThreadSafetyMode.ExecutionAndPublication),
            inputVisitor.InputProperties.ToArray());
    }

    private sealed class AmountExpression(
        Expression<Func<GameEntity, GameEntity, GameEntity?, float>> tree,
        Lazy<Func<GameEntity, GameEntity, GameEntity?, float>> function,
        string[] inputProperties)
    {
        public Expression<Func<GameEntity, GameEntity, GameEntity?, float>> Tree { get; } = tree;
        public Lazy<Func<GameEntity, GameEntity, GameEntity?, float>> Function { get; } = function;
        public string[] InputProperties { get; } = inputProperties;
    }

    private sealed class AmountInputExpressionVisitor : ExpressionVisitor
    {
        public List<string> InputProperties { get; } = [];

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member is PropertyInfo property
                && BeingProperties.ContainsKey(property.Name)
                && node.Expression is MemberExpression { Member: PropertyInfo beingProperty, Expression: var owner }
                && beingProperty == GameEntityBeingProperty
                && owner == ActivatorParameter)
            {
                Add(property.Name);
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            for (var i = 0; i < AmountInputModifiers.Length; i++)
            {
                var modifier = AmountInputModifiers[i];
                if (node.Method != modifier.Method)
                {
                    continue;
                }

                foreach (var property in modifier.Properties)
                {
                    Add(property);
                }

                break;
            }

            return base.VisitMethodCall(node);
        }

        private void Add(string property)
        {
            if (!InputProperties.Contains(property))
            {
                InputProperties.Add(property);
            }
        }
    }

    private static Expression CreateBeingPropertyAccess(Expression entity, PropertyInfo property)
        => Expression.Property(
            Expression.Property(entity, GameEntityBeingProperty),
            property);

    private class EffectExpressionVisitor : UnicornExpressionVisitor
    {
        public EffectExpressionVisitor(IEnumerable<ParameterExpression> parameters)
            : base(parameters)
        {
        }

        public override Expression VisitVariableExpression(
            UnicornExpressionParser.VariableExpressionContext context)
        {
            if (BeingProperties.TryGetValue(context.Variable.Text, out var property))
            {
                return CreateBeingPropertyAccess(ActivatorParameter, property);
            }

            return base.VisitVariableExpression(context);
        }

        public override Expression VisitAccessExpression(
            UnicornExpressionParser.AccessExpressionContext context)
        {
            if (context.Expression.GetText() == "$target")
            {
                if (!BeingProperties.TryGetValue(context.Member.Text, out var property))
                {
                    throw new InvalidOperationException(
                        $"Unknown target Being property '{context.Member.Text}'.");
                }

                return CreateBeingPropertyAccess(TargetParameter, property);
            }

            return base.VisitAccessExpression(context);
        }

        protected override Expression CreateInvocation(string function, IEnumerable<Expression> children,
            Type childrenCommonType)
        {
            MethodInfo method;
            switch (function)
            {
                case "RequirementsModifier":
                    method = _requirementsModifierMethod;
                    break;
                case "MightModifier":
                    method = _mightModifierMethod;
                    break;
                case "FocusModifier":
                    method = _focusModifierMethod;
                    break;
                case "PerceptionModifier":
                    method = _perceptionModifierMethod;
                    break;
                case "SpeedModifier":
                    method = _speedModifierMethod;
                    break;
                default:
                    return base.CreateInvocation(function, children, childrenCommonType);
            }

            if (children.Any())
            {
                throw new InvalidOperationException($"Expected 0 parameters for function {function}");
            }

            return Expression.Call(null, method, EffectParameter, ActivatorParameter);
        }
    }

    // Wires a self-targeted, non-instant modifier's magnitude into the dynamic-magnitude graph so it stays in
    // sync with the stats it is derived from: whenever a stat the amount expression reads changes on the
    // owner, the instance's magnitude is recomputed and its target property re-resolved. Only a self-cast
    // (owner == activator) stat-derived amount qualifies - the graph triggers on the owner's own property
    // changes and the recompute reads the owner's stats, so the two entities must coincide; a fixed override,
    // a constant amount, or an expression that reads only target-qualified properties is left frozen. A direct
    // self-reference to the property being fed is rejected. Called whenever such an instance is added or
    // updated, and unregisters an instance that no longer qualifies.
    private void TryRegisterDynamicMagnitude(
        EffectApplication application, ActivePropertyEffect instance,
        GameEntity targetEntity, GameEntity activatorEntity)
    {
        var activeEffects = targetEntity.ActiveEffects!;
        var amount = application.AmountExpression;

        if (activatorEntity != targetEntity
            || amount == null
            || instance.TargetName is not { } targetProperty)
        {
            activeEffects.UnregisterDynamicMagnitude(instance);
            return;
        }

        List<string>? tracked = null;
        foreach (var input in GetAmountInputProperties(amount))
        {
            if (input == targetProperty)
            {
                throw new InvalidOperationException(
                    $"Dynamic magnitude for '{targetProperty}' cannot depend on itself.");
            }

            (tracked ??= []).Add(input);
        }

        if (tracked == null)
        {
            activeEffects.UnregisterDynamicMagnitude(instance);
            return;
        }

        activeEffects.RegisterDynamicMagnitude(
            instance, tracked,
            () => application.GetAmount(this, targetEntity, targetEntity));
    }

    public void RebuildDynamicMagnitudes(GameEntity targetEntity)
    {
        var activeEffects = targetEntity.ActiveEffects!;
        activeEffects.ClearDynamicMagnitudes();

        foreach (var instance in activeEffects.PropertyEffects)
        {
            if (instance is
                {
                    SourceAbilityEntityId: null,
                    SourceAbilityType: AbilityType.Innate,
                    SourceAbilityActivation: ActivationType.Always,
                    TargetName: { } innateProperty
                })
            {
                var innateEffect = AbilityData.Innate.Effects?.FirstOrDefault(
                    effect => effect is IChangeProperty changeProperty
                              && changeProperty.PropertyName == innateProperty);
                if (innateEffect != null)
                {
                    var application = new EffectApplication(innateEffect, AbilityData.Innate);
                    TryRegisterDynamicMagnitude(application, instance, targetEntity, targetEntity);
                }

                continue;
            }

            if (instance.SourceAbilityEntityId is not { } sourceAbilityId
                || instance.TargetName is not { } targetProperty
                || targetEntity.Manager.FindEntity(sourceAbilityId)?.Ability is not { } sourceAbility)
            {
                continue;
            }

            foreach (var effectEntity in sourceAbility.Effects)
            {
                if (effectEntity.Effect is { EffectType: EffectType.ChangeProperty } effect
                    && effect.TargetName == targetProperty)
                {
                    TryRegisterDynamicMagnitude(
                        new EffectApplication(effect), instance, targetEntity, targetEntity);
                    break;
                }
            }
        }
    }
}
