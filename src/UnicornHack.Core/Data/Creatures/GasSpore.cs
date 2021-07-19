using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature GasSpore = new Creature
        {
            Name = "gas spore",
            Species = Species.FloatingSphere,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnRangedAttack,
                        Range = 20,
                        Action = AbilityAction.Explosion,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "140*FocusModifier()"}}
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnRangedAttack,
                        Range = 20,
                        Action = AbilityAction.Explosion,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect>
                        {
                            new Deafen {Duration = EffectDuration.UntilTimeout, DurationAmount = "27"}
                        }
                    }
                },
            InitialLevel = 3,
            GenerationWeight = "2",
            Sex = Sex.None,
            Size = 2,
            Weight = 10,
            MovementDelay = 300,
            TurningDelay = 300,
            Perception = -9,
            Might = -9,
            Speed = -9,
            Focus = -4,
            SlimingImmune = true,
            HeadType = HeadType.None,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Flying,
            SlotCapacity = 0,
            EyeCount = 0,
            NoiseLevel = 0,
            Mindless = true,
            NonAnimal = true
        };
    }
}
