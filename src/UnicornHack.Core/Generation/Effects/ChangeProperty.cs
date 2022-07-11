using System.Globalization;
using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class ChangeProperty<T> : DurationEffect
    where T : struct, IConvertible
{
    public string PropertyName
    {
        get;
        set;
    } = null!;

    // TODO: Add an amount string
    public T Value
    {
        get;
        set;
    }

    public ValueCombinationFunction Function
    {
        get;
        set;
    }

    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.ChangeProperty;
        effect.TargetName = PropertyName;
        effect.AppliedAmount = Value.ToInt32(CultureInfo.InvariantCulture);
        effect.CombinationFunction = Function;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<ChangeProperty<T>>(GetPropertyConditions<ChangeProperty<T>>());

    protected static new Dictionary<string, Func<TEffect, object?, bool>>
        GetPropertyConditions<TEffect>() where TEffect : Effect
    {
        var propertyConditions = DurationEffect.GetPropertyConditions<TEffect>();

        propertyConditions.Add(nameof(PropertyName), (_, v) => v != default);
        propertyConditions.Add(nameof(Value), (_, v) => !((T)v!).Equals(default(T)));
        propertyConditions.Add(nameof(Function), (_, v) => (ValueCombinationFunction)v! != default);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
