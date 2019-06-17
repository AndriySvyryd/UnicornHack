using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature BlackUnicorn = new Creature
        {
            Name = "black unicorn",
            Species = Species.Unicorn,
            SpeciesClass = SpeciesClass.Quadrupedal,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Headbutt,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "60*physicalScaling"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Kick,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "30*physicalScaling"}}
                }
            },
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            Behavior = AIBehavior.AlignmentAware | AIBehavior.RangedPeaceful | AIBehavior.Wandering |
                       AIBehavior.GemCollector,
            Noise = ActorNoiseType.Neigh,
            Size = 8,
            Weight = 1300,
            MovementDelay = -50,
            TurningDelay = -50,
            Perception = -7,
            Might = -8,
            Speed = -7,
            Focus = -8,
            Armor = 4,
            MagicResistance = 35,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            Infravisible = true
        };
    }
}
