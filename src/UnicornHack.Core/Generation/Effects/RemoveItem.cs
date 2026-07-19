using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Generation.Effects;

public class RemoveItem : Effect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Default(
            EffectType.RemoveItem, defaultDuration: EffectDuration.Instant,
            combinationFunction: ValueCombinationFunction.Sum,
            applicationHandler: Apply,
            resultMessageSelector: CreateResultMessage);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static void Apply(in EffectApplicationContext context)
    {
        GameEntity itemToRemove;
        if (context.Result is ItemActivationResultMessage consumedItemResult)
        {
            consumedItemResult.ItemConsumed = true;
            itemToRemove = consumedItemResult.ItemEntity;
        }
        else
        {
            itemToRemove = FindItem(context.Effect.Component, context.Manager);
        }
        using var itemReference = context.Manager.ItemMovingSystem.Split(itemToRemove.Item!, 1);
        EntityReferenceMessage<GameEntity>.Enqueue(itemReference.Referenced, context.Manager);
    }

    private static EffectResultMessage CreateResultMessage(
        EffectComponent effect, GameEntity _, GameManager manager)
    {
        var result = ItemActivationResultMessage.Create(manager);
        result.ItemEntity = FindItem(effect, manager);
        return result;
    }

    private static GameEntity FindItem(EffectComponent effect, GameManager manager)
    {
        var item = manager.FindEntity(effect.TargetEntityId);
        var containingAbilityEntity = effect.ContainingAbility;
        while (containingAbilityEntity != null)
        {
            item = containingAbilityEntity.Ability!.OwnerEntity;
            if (item?.HasComponent(EntityComponent.Item) != true)
            {
                containingAbilityEntity = containingAbilityEntity.Effect?.ContainingAbility;
            }
            else
            {
                break;
            }
        }

        if (item?.HasComponent(EntityComponent.Item) != true)
        {
            throw new InvalidOperationException("Couldn't find item to remove");
        }

        return item;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<RemoveItem>(GetPropertyConditions<RemoveItem>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
