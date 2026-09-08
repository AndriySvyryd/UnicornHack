namespace UnicornHack.Services.LogEvents;

public readonly struct LingeringAttackEvent
{
    public LingeringAttackEvent(
        GameEntity sensorEntity,
        GameEntity sourceEntity,
        GameEntity targetEntity,
        SenseType sourceSensed,
        SenseType targetSensed,
        IReadOnlyDictionary<EffectType, int> effects)
    {
        SensorEntity = sensorEntity;
        SourceEntity = sourceEntity;
        TargetEntity = targetEntity;
        SourceSensed = sourceSensed;
        TargetSensed = targetSensed;
        Effects = effects;
    }

    public GameEntity SensorEntity { get; }
    public GameEntity SourceEntity { get; }
    public GameEntity TargetEntity { get; }
    public SenseType SourceSensed { get; }
    public SenseType TargetSensed { get; }
    public IReadOnlyDictionary<EffectType, int> Effects { get; }
}
