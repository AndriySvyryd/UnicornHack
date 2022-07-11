namespace UnicornHack.Services.LogEvents;

public readonly struct ItemDropEvent
{
    public ItemDropEvent(
        GameEntity sensorEntity, GameEntity dropperEntity, GameEntity itemEntity,
        int quantity, SenseType dropperSensed, SenseType itemSensed)
    {
        SensorEntity = sensorEntity;
        DropperEntity = dropperEntity;
        ItemEntity = itemEntity;
        Quantity = quantity;
        DropperSensed = dropperSensed;
        ItemSensed = itemSensed;
    }

    public GameEntity SensorEntity
    {
        get;
    }

    public GameEntity DropperEntity
    {
        get;
    }

    public GameEntity ItemEntity
    {
        get;
    }

    public int Quantity
    {
        get;
    }

    public SenseType DropperSensed
    {
        get;
    }

    public SenseType ItemSensed
    {
        get;
    }
}
