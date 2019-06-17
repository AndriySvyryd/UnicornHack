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
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Bite,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "30*physicalScaling"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnRangedAttack,
                    Range = 20,
                    Action = AbilityAction.Gaze,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 350,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new Wither {Damage = "50*mentalScaling"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnRangedAttack,
                    Range = 20,
                    Action = AbilityAction.Gaze,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 350,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect>
                        {new Slow {Duration = EffectDuration.UntilTimeout, DurationAmount = "13"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnRangedAttack,
                    Range = 20,
                    Action = AbilityAction.Gaze,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 350,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect>
                        {new Sedate {Duration = EffectDuration.UntilTimeout, DurationAmount = "13"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnRangedAttack,
                    Range = 20,
                    Action = AbilityAction.Gaze,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 350,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect>
                        {new Confuse {Duration = EffectDuration.UntilTimeout, DurationAmount = "13"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnRangedAttack,
                    Range = 20,
                    Action = AbilityAction.Gaze,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 350,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect>
                    {
                        new Stone()
                    }
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnRangedAttack,
                    Range = 20,
                    Action = AbilityAction.Gaze,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 350,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new DrainEnergy {Amount = "3*mentalScaling"}}
                }
            },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
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
