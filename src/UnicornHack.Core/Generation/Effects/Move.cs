using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;

namespace UnicornHack.Generation.Effects;

public class Move : Effect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Default(
            EffectType.Move, defaultDuration: EffectDuration.Instant,
            combinationFunction: ValueCombinationFunction.Sum,
            defaultAmount: "1",
            applicationHandler: Apply,
            resultMessageSelector: SelectResultMessage,
            appliesOnFailure: true);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static void Apply(in EffectApplicationContext context)
    {
        if (context.Pretend)
        {
            return;
        }

        var effect = context.Effect.Component;
        var movee = effect.TargetEntityId is { } entityId
            ? context.Manager.FindEntity(entityId)
            : context.TargetEntity;
        if (movee?.Item == null)
        {
            return;
        }

        var moveItemMessage = MoveItemMessage.Create(context.Manager);
        moveItemMessage.ItemEntity = movee;
        moveItemMessage.SuppressLog = context.TargetEntity != context.ActivatorEntity;
        moveItemMessage.Force = true;

        if (context.TargetEntity != null)
        {
            moveItemMessage.TargetRoomEntity = context.TargetEntity.Position!.RoomEntity;
            moveItemMessage.TargetCell = context.TargetEntity.Position.Cell;
        }
        else
        {
            Debug.Assert(context.TargetCell != null);
            moveItemMessage.TargetRoomEntity = context.ActivatorEntity.Position!.RoomEntity;
            moveItemMessage.TargetCell = context.TargetCell;
        }

        context.Manager.Enqueue(moveItemMessage);
    }

    private static EffectResultMessage? SelectResultMessage(
        EffectComponent effect, GameEntity _, GameManager manager)
    {
        if (manager.FindEntity(effect.TargetEntityId) is not { Item: not null } itemEntity)
        {
            return null;
        }

        var result = ItemActivationResultMessage.Create(manager);
        result.ItemEntity = itemEntity;
        return result;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Move>(GetPropertyConditions<Move>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
