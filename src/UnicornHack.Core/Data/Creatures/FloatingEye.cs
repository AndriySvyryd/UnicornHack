using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature FloatingEye = new Creature
        {
            Name = "floating eye",
            Species = Species.FloatingSphere,
            SpeciesClass = SpeciesClass.Aberration,
            MovementDelay = 1200,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnMeleeHit,
                    Effects = new HashSet<Effect>
                        {new Paralyze {Duration = EffectDuration.UntilTimeout, DurationAmount = "35"}}
                }
            },
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Behavior = AIBehavior.Wandering,
            Sex = Sex.None,
            Size = 2,
            Weight = 10,
            Perception = 2,
            Might = 2,
            Speed = 2,
            Focus = 2,
            MagicResistance = 5,
            HeadType = HeadType.None,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Flying,
            InventorySize = 0,
            NoiseLevel = 0,
            Infravisible = true,
            Infravision = true,
            Mindless = true,
            NonAnimal = true
        };
    }
}
