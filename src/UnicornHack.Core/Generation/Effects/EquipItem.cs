using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class EquipItem : Effect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Default(
            EffectType.EquipItem, defaultDuration: EffectDuration.Instant,
            combinationFunction: ValueCombinationFunction.Sum,
            applicationHandler: Apply);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static void Apply(in EffectApplicationContext context)
    {
        var itemEntity = context.Manager.FindEntity(context.Effect.Component.TargetEntityId!.Value)!;
        foreach (var slot in context.Manager.ItemUsageSystem.GetEquipableSlots(
                     itemEntity.Item!, context.TargetEntity!))
        {
            if (!context.Manager.ItemUsageSystem.TryEquip(
                    itemEntity, slot, context.TargetEntity!, force: false))
            {
                break;
            }
        }
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<EquipItem>(GetPropertyConditions<EquipItem>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
