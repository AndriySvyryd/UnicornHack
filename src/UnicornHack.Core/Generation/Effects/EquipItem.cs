using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class EquipItem : Effect
{
    protected override void ConfigureEffect(EffectComponent effect)
        => effect.EffectType = EffectType.EquipItem;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<EquipItem>(GetPropertyConditions<EquipItem>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
