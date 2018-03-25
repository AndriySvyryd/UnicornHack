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
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 100}}
                }
            },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
            Size = 8,
            Weight = 900,
            Agility = 5,
            Constitution = 5,
            Intelligence = 5,
            Quickness = 5,
            Strength = 5,
            Willpower = 5,
            PhysicalDeflection = 17,
            TorsoType = TorsoType.Quadruped,
            UpperExtremeties = ExtremityType.Claws,
            LowerExtremeties = ExtremityType.Claws,
            LocomotionType = LocomotionType.Flying,
            NoiseLevel = 0,
            VisibilityLevel = 0,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}
