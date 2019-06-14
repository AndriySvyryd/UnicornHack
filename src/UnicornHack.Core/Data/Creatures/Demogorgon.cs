using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Demogorgon = new Creature
        {
            Name = "Demogorgon",
            Species = Species.DemonMajor,
            SpeciesClass = SpeciesClass.Demon,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 20,
                    Action = AbilityAction.Spell,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new ScriptedEffect {Script = "ArcaneSpell"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Sting,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new DrainLife {Amount = "20*physicalScaling"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Claw,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect>
                    {
                        new Burn {Damage = "20*physicalScaling"},
                        new ChangeProperty<int>
                        {
                            PropertyName = "Might", Value = -3, Duration = EffectDuration.UntilTimeout,
                            DurationAmount = "1000"
                        }
                    }
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Claw,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "25*physicalScaling"}}
                }
            },
            InitialLevel = 30,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight {Multiplier = 0F}, Name = "hell"},
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.Stalking,
            Noise = ActorNoiseType.Growl,
            Sex = Sex.Male,
            Size = 8,
            Weight = 1500,
            MovementDelay = -20,
            TurningDelay = -20,
            Perception = 6,
            Might = 6,
            Speed = 6,
            Focus = 6,
            Armor = 9,
            MagicResistance = 47,
            FireResistance = 75,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            LocomotionType = LocomotionType.Flying,
            Infravisible = true,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}
