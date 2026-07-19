using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class AddAbility : Effect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Default(
            EffectType.AddAbility, defaultDuration: EffectDuration.Infinite,
            combinationFunction: ValueCombinationFunction.Sum,
            defaultAmount: "1",
            applicationHandler: Apply);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public Ability? Ability
    {
        get;
        set;
    }

    public string Name
    {
        get;
        set;
    } = null!;

    public override EffectMetadata Metadata => _metadata;

    private static void Apply(in EffectApplicationContext context)
    {
        var addedName = context.Effect.Component.TargetName;
        Debug.Assert(addedName != null);

        context.System.ApplyAddAbility(
            context,
            addedName,
            context.Amount);
    }

    protected override void ConfigureEffect(EffectComponent effect, string? secondaryAmountOverride)
    {
        base.ConfigureEffect(effect, secondaryAmountOverride);

        if (Ability == null)
        {
            effect.TargetName = Name;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(Ability.Name))
            {
                throw new InvalidOperationException(
                    $"An ability embedded in '{ContainingAbility.Name}' must have a name.");
            }

            effect.TargetName = Ability.Name;
        }

        Ability?.AddToEffect(effect.Entity);
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<AddAbility>(GetPropertyConditions<AddAbility>());

    protected static new Dictionary<string, Func<TEffect, object?, bool>>
        GetPropertyConditions<TEffect>() where TEffect : Effect
    {
        var propertyConditions = Effect.GetPropertyConditions<TEffect>();

        propertyConditions.Add(nameof(Ability), (_, v) => v != default);
        propertyConditions.Add(nameof(Name), (_, v) => v != default);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
