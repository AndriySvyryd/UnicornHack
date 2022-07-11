using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class ScriptedEffect : Effect
{
    public string Script
    {
        get;
        set;
    } = null!;

    protected override void ConfigureEffect(EffectComponent effect)
    {
        // TODO: Add a specific effect type
        effect.EffectType = EffectType.Heal;
        effect.Amount = "0";
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<ScriptedEffect>(GetPropertyConditions<ScriptedEffect>());

    protected static new Dictionary<string, Func<TEffect, object?, bool>>
        GetPropertyConditions<TEffect>() where TEffect : Effect
    {
        var propertyConditions = Effect.GetPropertyConditions<TEffect>();

        propertyConditions.Add(nameof(Script), (_, v) => v != default);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
