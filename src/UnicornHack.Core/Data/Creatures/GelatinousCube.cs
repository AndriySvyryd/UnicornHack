using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature GelatinousCube = new Creature
        {
            Name = "gelatinous cube",
            Species = Species.Blob,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnMeleeAttack,
                        Range = 1,
                        Action = AbilityAction.Touch,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect>
                        {
                            new Paralyze {Duration = EffectDuration.UntilTimeout, DurationAmount = "4"}
                        }
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnMeleeAttack,
                        Range = 1,
                        Action = AbilityAction.Touch,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 350,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect>
                        {
                            new Engulf {Duration = EffectDuration.UntilTimeout, DurationAmount = "7"}
                        }
                    },
                    new Ability
                    {
                        Activation = ActivationType.OnDigestion,
                        Action = AbilityAction.Digestion,
                        Cooldown = 100,
                        Effects = new List<Effect>
                        {
                            new Blight {Damage = "10*MightModifier()"},
                            new Corrode {Damage = "10*MightModifier()"}
                        }
                    },
                    new Ability
                    {
                        Activation = ActivationType.OnMeleeHit,
                        Action = AbilityAction.Touch,
                        SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                        Accuracy = "10+PerceptionModifier()",
                        Effects = new List<Effect>
                        {
                            new Paralyze
                            {
                                Duration = EffectDuration.UntilTimeout, DurationAmount = "200"
                            }
                        }
                    }
                },
            InitialLevel = 6,
            GenerationWeight = "4",
            Behavior = AIBehavior.Wandering | AIBehavior.WeaponCollector,
            Sex = Sex.None,
            Size = 8,
            Weight = 600,
            MovementDelay = 100,
            TurningDelay = 100,
            Perception = -6,
            Might = -6,
            Speed = -6,
            Focus = -2,
            Armor = 1,
            AcidResistance = 75,
            ColdResistance = 75,
            ElectricityResistance = 75,
            FireResistance = 75,
            StoningImmune = true,
            HeadType = HeadType.None,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            EyeCount = 0,
            NoiseLevel = 0,
            Mindless = true,
            NonAnimal = true
        };
    }
}
