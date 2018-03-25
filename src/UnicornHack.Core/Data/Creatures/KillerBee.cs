using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature KillerBee = new Creature
        {
            Name = "killer bee",
            Species = Species.Bee,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 66,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Sting,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Blight {Damage = 20}}
                }
            },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            GenerationFlags = GenerationFlags.LargeGroup,
            Noise = ActorNoiseType.Buzz,
            Sex = Sex.Female,
            Size = 1,
            Weight = 5,
            Agility = 1,
            Constitution = 1,
            Intelligence = 1,
            Quickness = 1,
            Strength = 1,
            Willpower = 1,
            PhysicalDeflection = 21,
            TorsoType = TorsoType.Quadruped,
            UpperExtremeties = ExtremityType.None,
            LowerExtremeties = ExtremityType.Claws,
            LocomotionType = LocomotionType.Flying
        };
    }
}
