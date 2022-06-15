using UnicornHack.Primitives;

namespace UnicornHack.Services.LogEvents;

public readonly struct ItemPickUpEvent
{
    public ItemPickUpEvent(
        GameEntity sensorEntity, GameEntity pickerEntity, GameEntity itemEntity,
        int quantity, SenseType pickerSensed, SenseType itemSensed)
    {
        SensorEntity = sensorEntity;
        PickerEntity = pickerEntity;
        ItemEntity = itemEntity;
        Quantity = quantity;
        PickerSensed = pickerSensed;
        ItemSensed = itemSensed;
    }

    public GameEntity SensorEntity
    {
        get;
    }

    public GameEntity PickerEntity
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

    public SenseType PickerSensed
    {
        get;
    }

    public SenseType ItemSensed
    {
        get;
    }
}
