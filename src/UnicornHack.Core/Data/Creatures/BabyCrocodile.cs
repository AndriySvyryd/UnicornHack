using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature BabyCrocodile = new Creature
        {
            Name = "baby crocodile",
            Species = Species.Crocodile,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 200,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 60}}
                }
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            NextStageName = "crocodile",
            Size = 2,
            Weight = 200,
            Agility = 4,
            Constitution = 4,
            Intelligence = 4,
            Quickness = 4,
            Strength = 4,
            Willpower = 4,
            PhysicalDeflection = 14,
            UpperExtremeties = ExtremityType.None,
            RespirationType = RespirationType.Amphibious,
            LocomotionType = LocomotionType.Swimming,
            InventorySize = 1
        };
    }
}
