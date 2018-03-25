using UnicornHack.Primitives;

namespace UnicornHack.Services.LogEvents
{
    public readonly struct ItemEquipmentEvent
    {
        public ItemEquipmentEvent(
            GameEntity sensorEntity, GameEntity equipperEntity, GameEntity itemEntity,
            SenseType equipperSensed, SenseType itemSensed, EquipmentSlot slot)
        {
            SensorEntity = sensorEntity;
            EquipperEntity = equipperEntity;
            ItemEntity = itemEntity;
            EquipperSensed = equipperSensed;
            ItemSensed = itemSensed;
            Slot = slot;
        }

        public GameEntity SensorEntity { get; }
        public GameEntity EquipperEntity { get; }
        public GameEntity ItemEntity { get; }
        public SenseType EquipperSensed { get; }
        public SenseType ItemSensed { get; }
        public EquipmentSlot Slot { get; }
    }
}
