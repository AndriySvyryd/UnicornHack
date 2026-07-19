using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Abilities;

namespace UnicornHack.Generation.Effects;

public class Activate : Effect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Default(
            EffectType.Activate, defaultDuration: EffectDuration.Instant,
            combinationFunction: ValueCombinationFunction.Sum,
            applicationHandler: Apply,
            resultMessageSelector: SelectResultMessage,
            appliesOnFailure: true);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public string? Projectile
    {
        get;
        set;
    }

    /// <summary>
    ///     When set, this activates the weapon the actor wields in the given equipment slot rather than a
    ///     fixed entity - the slot is resolved to the current weapon (equipped item or natural weapon) when
    ///     the attack fires - so the same definition can be shared between wielders.
    /// </summary>
    public EquipmentSlot WeaponSlot
    {
        get;
        set;
    }

    public override EffectMetadata Metadata => _metadata;

    private static void Apply(in EffectApplicationContext context)
    {
        var effect = context.Effect.Component;
        Debug.Assert(effect.Duration != EffectDuration.Infinite
                     || context.Manager.FindEntity(effect.TargetEntityId)!.Ability!.IsActive);
    }

    private static EffectResultMessage SelectResultMessage(
        EffectComponent effect, GameEntity activatorEntity, GameManager manager)
    {
        var activatedEntity = AbilityActivationSystem.ResolveActivationTarget(
            effect.TargetEntityId!.Value, activatorEntity, manager)
            ?? throw new InvalidOperationException(
                $"Couldn't find the entity to activate '{effect.TargetEntityId}'"
                + $" referenced from effect '{effect.EntityId}'");
        if (effect.TargetEntityId < 0
            || activatedEntity.HasComponent(EntityComponent.Item)
            || activatedEntity.Ability?.OwnerEntity?.Item == null)
        {
            var attackResult = AttackResultMessage.Create(manager);
            if (effect.TargetEntityId < 0
                || activatedEntity.HasComponent(EntityComponent.Item))
            {
                attackResult.WeaponEntity = activatedEntity;
            }

            return attackResult;
        }

        var itemResult = ItemActivationResultMessage.Create(manager);
        itemResult.ItemEntity = activatedEntity.Ability.OwnerEntity;
        return itemResult;
    }

    protected override void ConfigureEffect(EffectComponent effect, string? secondaryAmountOverride)
    {
        base.ConfigureEffect(effect, secondaryAmountOverride);

        if (Projectile != null)
        {
            var item = effect.Entity.Manager.Game.Services.Data.Items.Get(Projectile).AddToEntity(effect.Entity);
            effect.TargetEntityId = item.EntityId;
        }
        else if (WeaponSlot != EquipmentSlot.None)
        {
            effect.TargetEntityId = -(int)WeaponSlot;
        }
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Activate>(GetPropertyConditions<Activate>());

    protected static new Dictionary<string, Func<TEffect, object?, bool>>
        GetPropertyConditions<TEffect>() where TEffect : Effect
    {
        var propertyConditions = Effect.GetPropertyConditions<TEffect>();

        propertyConditions.Add(nameof(Projectile), (_, v) => v != default);
        propertyConditions.Add(nameof(WeaponSlot), (_, v) => (EquipmentSlot)v! != default);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
