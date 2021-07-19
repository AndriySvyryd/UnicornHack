using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Lemure = new Creature
        {
            Name = "lemure",
            Species = Species.Ghost,
            SpeciesClass = SpeciesClass.Undead,
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
                            new Paralyze {Duration = EffectDuration.UntilTimeout, DurationAmount = "7"}
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
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect>
                        {
                            new Slow {Duration = EffectDuration.UntilTimeout, DurationAmount = "3"}
                        }
                    }
                },
            InitialLevel = 12,
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
            Noise = ActorNoiseType.Howl,
            Weight = 0,
            MovementDelay = 20,
            TurningDelay = 20,
            Material = Material.Air,
            Perception = -3,
            Might = -4,
            Speed = -3,
            Focus = 2,
            PhysicalResistance = 50,
            MagicResistance = 12,
            ColdResistance = 75,
            VoidResistance = 75,
            SlimingImmune = true,
            StoningImmune = true,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Walking | LocomotionType.Phasing,
            SlotCapacity = 0,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}
