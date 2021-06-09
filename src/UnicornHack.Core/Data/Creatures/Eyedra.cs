using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Eyedra = new Creature
        {
            Name = "eyedra",
            Species = Species.FloatingSphere,
            SpeciesClass = SpeciesClass.Aberration,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnMeleeAttack,
                        Range = 1,
                        Action = AbilityAction.Bite,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "30*MightModifier()"}}
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnRangedAttack,
                        Range = 20,
                        Action = AbilityAction.Gaze,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 350,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new Wither {Damage = "50*FocusModifier()"}}
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnRangedAttack,
                        Range = 20,
                        Action = AbilityAction.Gaze,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 350,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect>
                        {
                            new Slow {Duration = EffectDuration.UntilTimeout, DurationAmount = "13"}
                        }
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnRangedAttack,
                        Range = 20,
                        Action = AbilityAction.Gaze,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 350,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect>
                        {
                            new Sedate {Duration = EffectDuration.UntilTimeout, DurationAmount = "13"}
                        }
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnRangedAttack,
                        Range = 20,
                        Action = AbilityAction.Gaze,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 350,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect>
                        {
                            new Confuse {Duration = EffectDuration.UntilTimeout, DurationAmount = "13"}
                        }
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnRangedAttack,
                        Range = 20,
                        Action = AbilityAction.Gaze,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 350,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new Stone()}
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnRangedAttack,
                        Range = 20,
                        Action = AbilityAction.Gaze,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 350,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new DrainEnergy {Amount = "3*FocusModifier()"}}
                    }
                },
            InitialLevel = 8,
            GenerationWeight = "3",
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.Wandering,
            Sex = Sex.None,
            Weight = 250,
            MovementDelay = 200,
            TurningDelay = 200,
            Perception = -5,
            Might = -6,
            Speed = -5,
            Focus = -6,
            Armor = 3,
            MagicResistance = 17,
            ColdResistance = 75,
            HeadType = HeadType.None,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Flying,
            InventorySize = 0,
            NoiseLevel = 0,
            Infravisible = true,
            Infravision = true
        };
    }
}
