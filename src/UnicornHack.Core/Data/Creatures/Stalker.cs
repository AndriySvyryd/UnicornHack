using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Stalker = new Creature
        {
            Name = "stalker",
            Species = Species.Elemental,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Claw,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 100}}
                }
            },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
            Size = 8,
            Weight = 900,
            Perception = 5,
            Might = 4,
            Speed = 5,
            Focus = 4,
            PhysicalDeflection = 17,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.Claws,
            LowerExtremities = ExtremityType.Claws,
            LocomotionType = LocomotionType.Flying,
            NoiseLevel = 0,
            VisibilityLevel = 0,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}
