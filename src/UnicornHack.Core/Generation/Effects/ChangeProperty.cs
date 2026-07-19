using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public interface IChangeProperty
{
    string PropertyName { get; }
}

internal static class ChangePropertyMetadata
{
    internal static readonly EffectMetadata Metadata = EffectMetadata.Default(
        EffectType.ChangeProperty,
        defaultDuration: EffectDuration.Infinite,
        combinationFunction: ValueCombinationFunction.Default,
        applicationHandler: static (in context) =>
            context.System.ApplyChangeProperty(context));

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = Metadata;
}

public class ChangeProperty<T> : Effect, IChangeProperty
    where T : struct, IConvertible
{
    public string PropertyName
    {
        get;
        set;
    } = null!;

    public override EffectMetadata Metadata => GetMetadata(EffectType.ChangeProperty);

    protected override void ConfigureEffect(EffectComponent effect, string? secondaryAmountOverride)
    {
        base.ConfigureEffect(effect, secondaryAmountOverride);

        effect.TargetName = PropertyName;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<ChangeProperty<T>>(GetPropertyConditions<ChangeProperty<T>>());

    protected static new Dictionary<string, Func<TEffect, object?, bool>>
        GetPropertyConditions<TEffect>() where TEffect : Effect
    {
        var propertyConditions = Effect.GetPropertyConditions<TEffect>();

        propertyConditions.Add(nameof(PropertyName), (_, v) => v != default);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
