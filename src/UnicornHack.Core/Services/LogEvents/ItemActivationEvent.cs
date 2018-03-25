using UnicornHack.Primitives;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Services.LogEvents
{
    public readonly struct ItemActivationEvent
    {
        public ItemActivationEvent(
            GameEntity sensorEntity, GameEntity itemEntity, GameEntity activatorEntity, GameEntity targetEntity,
            Point? targetCell, SenseType itemSensed, SenseType activatorSensed, SenseType targetSensed,
            ActivationType activationType, bool successful)
        {
            SensorEntity = sensorEntity;
            ItemEntity = itemEntity;
            ActivatorEntity = activatorEntity;
            TargetEntity = targetEntity;
            TargetCell = targetCell;
            ItemSensed = itemSensed;
            ActivatorSensed = activatorSensed;
            TargetSensed = targetSensed;
            ActivationType = activationType;
            Successful = successful;
        }

        public GameEntity SensorEntity { get; }
        public GameEntity ItemEntity { get; }
        public GameEntity ActivatorEntity { get; }
        public GameEntity TargetEntity { get; }
        public Point? TargetCell { get; }
        public SenseType ItemSensed { get; }
        public SenseType ActivatorSensed { get; }
        public SenseType TargetSensed { get; }
        public ActivationType ActivationType { get; }
        public bool Successful { get; }
    }
}
