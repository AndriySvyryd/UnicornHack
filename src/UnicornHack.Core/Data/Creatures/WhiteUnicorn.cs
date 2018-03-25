using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature WhiteUnicorn = new Creature
        {
            Name = "white unicorn",
            Species = Species.Unicorn,
            SpeciesClass = SpeciesClass.Quadrupedal,
            MovementDelay = 50,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Headbutt,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 60}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Kick,
                    Timeout = 1,
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
            Agility = 3,
            Constitution = 3,
            Intelligence = 3,
            Quickness = 3,
            Strength = 3,
            Willpower = 3,
            MagicResistance = 70,
            PhysicalDeflection = 18,
            TorsoType = TorsoType.Quadruped,
            UpperExtremeties = ExtremityType.None,
            LowerExtremeties = ExtremityType.Claws,
            Infravisible = true
        };
    }
}
