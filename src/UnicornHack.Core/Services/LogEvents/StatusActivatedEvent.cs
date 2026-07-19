namespace UnicornHack.Services.LogEvents;

public readonly struct StatusActivatedEvent
{
    public StatusActivatedEvent(
        GameEntity sensorEntity, GameEntity targetEntity, EffectType effectType, string statusName, int abilityLevel,
        SenseType targetSensed)
    {
        SensorEntity = sensorEntity;
        TargetEntity = targetEntity;
        EffectType = effectType;
        StatusName = statusName;
        AbilityLevel = abilityLevel;
        TargetSensed = targetSensed;
    }

    public GameEntity SensorEntity
    {
        get;
    }

    public GameEntity TargetEntity
    {
        get;
    }

    public EffectType EffectType
    {
        get;
    }

    public string StatusName
    {
        get;
    }

    public int AbilityLevel
    {
        get;
    }

    public SenseType TargetSensed
    {
        get;
    }
}
