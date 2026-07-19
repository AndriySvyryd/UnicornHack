namespace UnicornHack.Systems.Effects;

public delegate void EffectApplicationHandler(in EffectApplicationContext context);

public readonly struct EffectApplicationContext
{
    public EffectApplicationContext(
        EffectApplicationSystem system,
        GameEntity effectEntity,
        GameEntity? targetEntity,
        Point? targetCell,
        GameEntity activatorEntity,
        EffectResultMessage? result,
        bool pretend,
        int? resolvedAmount = null)
    {
        System = system;
        Effect = new EffectApplication(effectEntity.Effect!, resolvedAmount);
        TargetEntity = targetEntity;
        TargetCell = targetCell;
        ActivatorEntity = activatorEntity;
        Result = result;
        Manager = activatorEntity.Manager;
        Pretend = pretend;
    }

    private EffectApplicationContext(
        EffectApplicationSystem system, EffectType effectType, GameEntity targetEntity,
        int amount, int? sourceAbilityId)
    {
        System = system;
        Effect = EffectApplication.CreateDirect(effectType, amount, sourceAbilityId);
        TargetEntity = targetEntity;
        TargetCell = null;
        ActivatorEntity = targetEntity;
        Result = null;
        Manager = targetEntity.Manager;
        Pretend = false;
    }

    public static EffectApplicationContext CreateStatus(
        EffectType effectType, GameEntity targetEntity, int amount, int? sourceAbilityId = null)
        => new(targetEntity.Manager.EffectApplicationSystem, effectType, targetEntity, amount, sourceAbilityId);

    public EffectApplicationSystem System { get; }
    public EffectApplication Effect { get; }
    public int Amount => Effect.GetAmount(System, ActivatorEntity, TargetEntity);
    public int DurationAmount => Effect.GetDurationAmount(System, ActivatorEntity, TargetEntity);
    public int SecondaryAmount => Effect.GetSecondaryAmount(System, ActivatorEntity, TargetEntity);
    public GameEntity? TargetEntity { get; }
    public Point? TargetCell { get; }
    public GameEntity ActivatorEntity { get; }
    public EffectResultMessage? Result { get; }
    public GameManager Manager { get; }
    public bool Pretend { get; }
}
