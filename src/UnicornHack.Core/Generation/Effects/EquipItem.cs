using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class EquipItem : Effect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Default(
            EffectType.EquipItem, defaultDuration: EffectDuration.Instant,
            combinationFunction: ValueCombinationFunction.Sum,
            applicationHandler: Apply,
            resultMessageSelector: CreateResultMessage);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static void Apply(in EffectApplicationContext context)
    {
        var itemEntity = context.Manager.FindEntity(context.Effect.Component.TargetEntityId!.Value)!;
        var equipped = false;
        foreach (var slot in context.Manager.ItemUsageSystem.GetEquipableSlots(
                     itemEntity.Item!, context.TargetEntity!))
        {
            if (!context.Manager.ItemUsageSystem.TryEquip(
                    itemEntity, slot, context.TargetEntity!, force: false))
            {
                equipped = true;
                break;
            }
        }

        if (!equipped)
        {
            foreach (var slot in context.Manager.ItemUsageSystem.GetEquipableSlots(
                         itemEntity.Item!, context.TargetEntity!))
            {
                if (!context.Manager.ItemUsageSystem.TryEquip(
                        itemEntity, slot, context.TargetEntity!, force: true))
                {
                    break;
                }
            }
        }
    }

    private static EffectResultMessage CreateResultMessage(
        EffectComponent effect, GameEntity _, GameManager manager)
    {
        var result = ItemActivationResultMessage.Create(manager);
        result.ItemEntity = manager.FindEntity(effect.TargetEntityId)!;
        return result;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<EquipItem>(GetPropertyConditions<EquipItem>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
