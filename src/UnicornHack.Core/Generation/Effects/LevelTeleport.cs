using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class LevelTeleport : Effect
{
    protected override void ConfigureEffect(EffectComponent effect)
        => effect.EffectType = EffectType.LevelTeleport;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<LevelTeleport>(GetPropertyConditions<LevelTeleport>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
