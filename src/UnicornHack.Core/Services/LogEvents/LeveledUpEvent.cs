using UnicornHack.Systems.Beings;

namespace UnicornHack.Services.LogEvents
{
    public readonly struct LeveledUpEvent
    {
        public LeveledUpEvent(
            GameEntity sensorEntity,
            GameEntity entity,
            RaceComponent race,
            int skillPointsGained,
            int traitPointsGained,
            int mutationPointsGained)
        {
            SensorEntity = sensorEntity;
            LeveledEntity = entity;
            Race = race;
            SkillPointsGained = skillPointsGained;
            TraitPointsGained = traitPointsGained;
            MutationPointsGained = mutationPointsGained;
        }

        public GameEntity SensorEntity { get; }
        public GameEntity LeveledEntity { get; }
        public RaceComponent Race { get; }
        public int SkillPointsGained { get; }
        public int TraitPointsGained { get; }
        public int MutationPointsGained { get; }
    }
}
