using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature GreaterNaga = new Creature
        {
            Name = "greater naga",
            Species = Species.Naga,
            SpeciesClass = SpeciesClass.Aberration,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = ActivationType.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "50"}}
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnMeleeAttack,
                        Range = 1,
                        Action = AbilityAction.Punch,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "10*MightModifier()"}}
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnMeleeAttack,
                        Range = 1,
                        Action = AbilityAction.Punch,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "10*MightModifier()"}}
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnMeleeAttack,
                        Range = 1,
                        Action = AbilityAction.Punch,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "50*MightModifier()"}}
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnMeleeAttack,
                        Range = 1,
                        Action = AbilityAction.Punch,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "50*MightModifier()"}}
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnMeleeAttack,
                        Range = 1,
                        Action = AbilityAction.Punch,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "50*MightModifier()"}}
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnMeleeAttack,
                        Range = 1,
                        Action = AbilityAction.Punch,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "50*MightModifier()"}}
                    }
                },
            InitialLevel = 9,
            GenerationWeight = "$branch == 'hell' ? 1 : 0",
            PreviousStageName = "naga",
            GenerationFlags = GenerationFlags.NonGenocidable,
            Behavior = AIBehavior.Stalking | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Cuss,
            Sex = Sex.Female,
            Size = 8,
            Weight = 1200,
            Perception = -5,
            Might = -6,
            Speed = -5,
            Focus = -6,
            Armor = 8,
            MagicResistance = 40,
            FireResistance = 75,
            TorsoType = TorsoType.Serpentine,
            LowerExtremities = ExtremityType.None,
            Infravisible = true,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}
