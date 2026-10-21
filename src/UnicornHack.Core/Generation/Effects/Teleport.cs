using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Teleport : Effect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Default(
        EffectType.Teleport, defaultDuration: EffectDuration.Instant,
        combinationFunction: ValueCombinationFunction.Sum,
        applicationHandler: Apply);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static void Apply(in EffectApplicationContext context)
    {
        if (context.Pretend || context.TargetEntity is not { Being.IsAlive: true } target
            || context.TargetCell is not { } targetCell)
        {
            return;
        }

        if (context.Effect.Component.ShouldTargetActivator
            && target.Position is { Cell: { } origin } position
            && !context.Manager.TravelSystem.IsActorFreeCell(targetCell, position.RoomEntity.Room!))
        {
            var destination = context.Manager.TravelSystem.FindCell(
                position.RoomEntity.Room!, context.Manager.TravelSystem.IsActorFreeCell,
                origin: targetCell, maxRadius: 1,
                direction: targetCell == origin ? position.Heading : targetCell.DifferenceTo(origin).GetUnit().AsDirection());
            if (destination == null)
            {
                return;
            }

            targetCell = destination.Value;
        }

        if (context.Manager.TravelSystem.TeleportActor(target, targetCell, context.Manager)
            && context.Result != null)
        {
            context.Result.AnyEffectApplied = true;
        }
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Teleport>(GetPropertyConditions<Teleport>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
