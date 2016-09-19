using System.Collections.Generic;
using static UnicornHack.Models.GameDefinitions.Skill;

namespace UnicornHack.Models.GameDefinitions
{
    public class PlayerVariant : ActorVariant
    {
        public static readonly PlayerVariant Human = new PlayerVariant(
            name: "player human", monsterVariant: MonsterVariant.Human, xpBonus: 1,
            strengthBonus: 2, dexterityBonus: 2, speedBonus: 2,
            constitutionBonus: 2, willpowerBonus: 2, intelligenceBonus: 2,
            skillAptitudes: new Dictionary<Skill, int>());

        public static readonly PlayerVariant Elf = new PlayerVariant(
            name: "player elf", monsterVariant: MonsterVariant.Elf, xpBonus: 0,
            strengthBonus: 1, dexterityBonus: 3, speedBonus: 2,
            constitutionBonus: 0, willpowerBonus: 2, intelligenceBonus: 2,
            skillAptitudes: new Dictionary<Skill, int> {{Bows, 1}});

        public PlayerVariant(
            string name,
            MonsterVariant monsterVariant,
            int xpBonus,
            int strengthBonus,
            int dexterityBonus,
            int speedBonus,
            int constitutionBonus,
            int intelligenceBonus,
            int willpowerBonus,
            Dictionary<Skill, int> skillAptitudes)
            : base(
                name, monsterVariant.Species, monsterVariant.InnateProperties, monsterVariant.MovementRate,
                monsterVariant.Size, monsterVariant.Weight, monsterVariant.Nutrition,
                monsterVariant.ConsumptionProperties, monsterVariant.SpeciesClass)
        {
            StrengthBonus = strengthBonus;
            DexterityBonus = dexterityBonus;
            SpeedBonus = speedBonus;
            ConstitutionBonus = constitutionBonus;
            IntelligenceBonus = intelligenceBonus;
            WillpowerBonus = willpowerBonus;
            SpeedBonus = speedBonus;
            SkillAptitudes = skillAptitudes;
        }

        public static readonly int StartingAttributeValue = 10;
        public virtual int StrengthBonus { get; }
        public virtual int DexterityBonus { get; }
        public virtual int ConstitutionBonus { get; }
        public virtual int IntelligenceBonus { get; }
        public virtual int WillpowerBonus { get; }
        public virtual int SpeedBonus { get; }
        public virtual IReadOnlyDictionary<Skill, int> SkillAptitudes { get; }
    }
}