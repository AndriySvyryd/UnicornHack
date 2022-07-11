namespace UnicornHack.Services.LogEvents;

public readonly struct DeathEvent
{
    public DeathEvent(GameEntity sensorEntity, GameEntity deceasedEntity, SenseType deceasedSensed)
    {
        SensorEntity = sensorEntity;
        DeceasedEntity = deceasedEntity;
        DeceasedSensed = deceasedSensed;
    }

    public GameEntity SensorEntity
    {
        get;
    }

    public GameEntity DeceasedEntity
    {
        get;
    }

    public SenseType DeceasedSensed
    {
        get;
    }
}
