namespace UnicornHack.Services.LogEvents;

public readonly struct ResourceEffectEvent
{
    public ResourceEffectEvent(
        GameEntity sensorEntity, GameEntity targetEntity, IReadOnlyDictionary<EffectType, int> resourceChanges,
        SenseType targetSensed)
    {
        SensorEntity = sensorEntity;
        TargetEntity = targetEntity;
        ResourceChanges = resourceChanges;
        TargetSensed = targetSensed;
    }

    public GameEntity SensorEntity { get; }

    public GameEntity TargetEntity { get; }

    public IReadOnlyDictionary<EffectType, int> ResourceChanges { get; }

    public SenseType TargetSensed { get; }
}