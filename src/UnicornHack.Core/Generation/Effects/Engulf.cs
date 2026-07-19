using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Engulf : ResistedEffect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Default(
            EffectType.Engulf, AbilityData.Engulfed.Name, applicationHandler: Apply,
            statusCategory: StatusCategory.Physical,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.EngulfResistance),
            statusAbilityDuration: "100*MightModifier()",
            combinationFunction: ValueCombinationFunction.BuildUpPerSource,
            defaultAmount: "200*MightModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static void Apply(in EffectApplicationContext context)
    {
        if (context.TargetEntity is not { } target
            || context.Manager.EngulferToVictimRelationship.GetVictim(context.ActivatorEntity) != null
            || context.Manager.EngulferToVictimRelationship.GetEngulfer(target) != null
            || context.ActivatorEntity.Position is not { Cell: { } activatorCell } activatorPosition
            || target.Position is not { Cell: { } targetCell } targetPosition
            || activatorPosition.RoomId != targetPosition.RoomId
            || activatorCell.DistanceTo(targetCell) != 1)
        {
            return;
        }

        context.System.ApplyStatus(context);
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Engulf>(GetPropertyConditions<Engulf>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
