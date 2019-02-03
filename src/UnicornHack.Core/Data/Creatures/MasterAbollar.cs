using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature MasterAbollar = new Creature
        {
            Name = "master abollar",
            Species = Species.Abollar,
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 40}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Punch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Suck,
                    Cooldown = 100,
                    Effects = new HashSet<Effect>
                    {
                        new ChangeProperty<int>
                        {
                            PropertyName = "Focus", Value = -2, Duration = EffectDuration.UntilTimeout,
                            DurationAmount = 10
                        }
                    }
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Suck,
                    Cooldown = 100,
                    Effects = new HashSet<Effect>
                    {
                        new ChangeProperty<int>
                        {
                            PropertyName = "Focus", Value = -2, Duration = EffectDuration.UntilTimeout,
                            DurationAmount = 10
                        }
                    }
                }
            },
            InitialLevel = 13,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            PreviousStageName = "abollar",
            Behavior = AIBehavior.GoldCollector | AIBehavior.GemCollector | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Gurgle,
            Size = 8,
            Weight = 1200,
            Perception = 7,
            Might = 6,
            Speed = 7,
            Focus = 6,
            MagicResistance = 45,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            LocomotionType = LocomotionType.Flying,
            Telepathic = 3,
            Infravisible = true,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}
