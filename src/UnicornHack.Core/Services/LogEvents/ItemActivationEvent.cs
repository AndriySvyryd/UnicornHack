namespace UnicornHack.Services.LogEvents;

public readonly struct ItemActivationEvent
{
    public ItemActivationEvent(
        GameEntity sensorEntity, GameEntity itemEntity, GameEntity activatorEntity, GameEntity targetEntity,
        SenseType itemSensed, SenseType activatorSensed, SenseType targetSensed, bool consumed, bool successful)
    {
        SensorEntity = sensorEntity;
        ItemEntity = itemEntity;
        ActivatorEntity = activatorEntity;
        TargetEntity = targetEntity;
        ItemSensed = itemSensed;
        ActivatorSensed = activatorSensed;
        TargetSensed = targetSensed;
        Consumed = consumed;
        Successful = successful;
    }

    public GameEntity SensorEntity
    {
        get;
    }

    public GameEntity ItemEntity
    {
        get;
    }

    public GameEntity ActivatorEntity
    {
        get;
    }

    public GameEntity TargetEntity
    {
        get;
    }

    public SenseType ItemSensed
    {
        get;
    }

    public SenseType ActivatorSensed
    {
        get;
    }

    public SenseType TargetSensed
    {
        get;
    }

    public bool Consumed
    {
        get;
    }

    public bool Successful
    {
        get;
    }
}
