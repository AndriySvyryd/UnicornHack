using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Sylph = new Creature
        {
            Name = "sylph",
            Species = Species.Elemental,
            SpeciesClass = SpeciesClass.Extraplanar,
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
                            new Engulf {Duration = EffectDuration.UntilTimeout, DurationAmount = "5"}
                        }
                    },
                    new Ability
                    {
                        Activation = ActivationType.OnDigestion,
                        Action = AbilityAction.Digestion,
                        Cooldown = 100,
                        Effects = new List<Effect>
                        {
                            new Deafen {Duration = EffectDuration.UntilTimeout, DurationAmount = "2"}
                        }
                    }
                },
            InitialLevel = 8,
            GenerationWeight = "2",
            Sex = Sex.None,
            Size = 16,
            Weight = 0,
            MovementDelay = -67,
            TurningDelay = -67,
            Material = Material.Air,
            Perception = -5,
            Might = -6,
            Speed = -5,
            Armor = 4,
            MagicResistance = 15,
            SlimingImmune = true,
            StoningImmune = true,
            HeadType = HeadType.None,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Flying,
            EyeCount = 0,
            VisibilityLevel = 0,
            Mindless = true,
            NonAnimal = true
        };
    }
}
