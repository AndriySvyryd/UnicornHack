using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Push : Effect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Default(
        EffectType.Push,
        combinationFunction: ValueCombinationFunction.Sum,
        defaultAmount: "200*MightModifier()",
        applicationHandler: Apply);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static void Apply(in EffectApplicationContext context)
    {
        if (context.Pretend || context.TargetEntity is not { Being.IsAlive: true } target)
        {
            return;
        }

        var amount = context.Amount;
        var status = EffectApplicationContext.CreateStatus(EffectType.Topple, target, amount);
        var buildup = context.System.GetEffectiveStatusAmount(status);
        context.System.BankBuildUp(EffectType.Topple, buildup, target, context.Manager);
        var moved = context.Manager.TravelSystem.PushActor(target, context.ActivatorEntity, amount, context.Manager);
        if ((buildup > 0 || moved > 0) && context.Result != null)
        {
            context.Result.AnyEffectApplied = true;
        }
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Push>(GetPropertyConditions<Push>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}