using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature GrayUnicorn = new Creature
        {
            Name = "gray unicorn",
            Species = Species.Unicorn,
            SpeciesClass = SpeciesClass.Quadrupedal,
            MovementDelay = 50,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Headbutt,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 60}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Kick,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                }
            },
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            Behavior = AIBehavior.AlignmentAware | AIBehavior.RangedPeaceful | AIBehavior.Wandering |
                       AIBehavior.GemCollector,
            Noise = ActorNoiseType.Neigh,
            Size = 8,
            Weight = 1300,
            Perception = 3,
            Might = 2,
            Speed = 3,
            Focus = 2,
            MagicDeflection = 35,
            PhysicalDeflection = 18,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            Infravisible = true
        };
    }
}
