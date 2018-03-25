using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature FlamingSphere = new Creature
        {
            Name = "flaming sphere",
            Species = Species.FloatingSphere,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 92,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Explosion,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Burn {Damage = 140}}
                }
            },
            InitialLevel = 6,
            GenerationWeight = new BranchWeight
            {
                NotMatched = new DefaultWeight {Multiplier = 4F},
                Name = "hell"
            },
            Sex = Sex.None,
            Size = 2,
            Weight = 10,
            Agility = 4,
            Constitution = 4,
            Intelligence = 4,
            Quickness = 4,
            Strength = 4,
            Willpower = 4,
            MagicResistance = 10,
            PhysicalDeflection = 16,
            FireResistance = 75,
            HeadType = HeadType.None,
            UpperExtremeties = ExtremityType.None,
            LowerExtremeties = ExtremityType.None,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Flying,
            InventorySize = 0,
            Infravisible = true,
            Mindless = true,
            NonAnimal = true
        };
    }
}
