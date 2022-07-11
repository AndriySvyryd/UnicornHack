using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class ChangeRace : DurationEffect
{
    public string RaceName
    {
        get;
        set;
    } = null!;

    public bool Remove
    {
        get;
        set;
    }

    public int Delay
    {
        get;
        set;
    }

    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.ChangeRace;
        effect.TargetName = RaceName;
        effect.AppliedAmount = Remove ? -1 : 1;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<ChangeRace>(GetPropertyConditions<ChangeRace>());

    protected static new Dictionary<string, Func<TEffect, object?, bool>>
        GetPropertyConditions<TEffect>() where TEffect : Effect
    {
        var propertyConditions = DurationEffect.GetPropertyConditions<TEffect>();

        propertyConditions.Add(nameof(RaceName), (o, v) => v != default);
        propertyConditions.Add(nameof(Remove), (o, v) => (bool)v! != default);
        propertyConditions.Add(nameof(Delay), (o, v) => (int)v! != default);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
