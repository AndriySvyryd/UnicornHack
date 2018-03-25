using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Crocodile = new Creature
        {
            Name = "crocodile",
            Species = Species.Crocodile,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 133,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 100}}
                }
            },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            PreviousStageName = "baby crocodile",
            Size = 8,
            Weight = 1500,
            Agility = 5,
            Constitution = 5,
            Intelligence = 5,
            Quickness = 5,
            Strength = 5,
            Willpower = 5,
            MagicResistance = 10,
            PhysicalDeflection = 16,
            UpperExtremeties = ExtremityType.None,
            RespirationType = RespirationType.Amphibious,
            LocomotionType = LocomotionType.Swimming,
            InventorySize = 1
        };
    }
}
