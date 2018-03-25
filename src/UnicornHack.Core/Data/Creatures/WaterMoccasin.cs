using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature WaterMoccasin = new Creature
        {
            Name = "water moccasin",
            Species = Species.Snake,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 80,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Blight {Damage = 30}}
                }
            },
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.LargeGroup,
            Noise = ActorNoiseType.Hiss,
            Size = 2,
            Weight = 150,
            Agility = 3,
            Constitution = 3,
            Intelligence = 3,
            Quickness = 3,
            Strength = 3,
            Willpower = 3,
            PhysicalDeflection = 17,
            TorsoType = TorsoType.Serpentine,
            UpperExtremeties = ExtremityType.None,
            LowerExtremeties = ExtremityType.None,
            LocomotionType = LocomotionType.Swimming,
            InventorySize = 0,
            VisibilityLevel = 2,
            Infravision = true
        };
    }
}
