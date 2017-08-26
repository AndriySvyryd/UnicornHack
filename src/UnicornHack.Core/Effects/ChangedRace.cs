using System;

namespace UnicornHack.Effects
{
    public class ChangedRace : AddedAbility
    {
        public ChangedRace()
        {
        }

        public ChangedRace(AbilityActivationContext abilityContext) : base(abilityContext)
        {
        }

        public bool Removed { get; set; }
        public string Name { get; set; }
        public Species Species { get; set; }
        public SpeciesClass SpeciesClass { get; set; }
        public byte XPLevel { get; set; }
        public int XP { get; set; }
        public int NextLevelXP { get; set; }

        public bool IsLearning => (Entity as Player)?.LearningRace == this;

        public void UpdateNextLevelXP()
        {
            if (!(Entity is Player player))
            {
                return;
            }
            var currentLevel = player.MaxXPLevel > XPLevel ? XPLevel : player.XPLevel;
            NextLevelXP = (int)((1 + Math.Ceiling(Math.Pow(currentLevel, 1.5))) * 50);
        }
    }
}