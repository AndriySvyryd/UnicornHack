using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Undine = new Creature
        {
            Name = "undine",
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
                        Action = AbilityAction.Punch,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "170*MightModifier()"}}
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
                        Effects = new List<Effect> {new Soak {Damage = "30*MightModifier()"}}
                    },
                    new Ability
                    {
                        Activation = ActivationType.OnMeleeHit,
                        Action = AbilityAction.Touch,
                        SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                        Accuracy = "10+PerceptionModifier()",
                        Effects = new List<Effect> {new Soak {Damage = "30*MightModifier()"}}
                    }
                },
            InitialLevel = 8,
            GenerationWeight = "2",
            Sex = Sex.Female,
            Size = 16,
            Weight = 2500,
            MovementDelay = 100,
            TurningDelay = 100,
            Material = Material.Water,
            Perception = -5,
            Might = -6,
            Speed = -5,
            BonusHitPoints = 70,
            Armor = 4,
            MagicResistance = 15,
            WaterResistance = 200,
            SlimingImmune = true,
            StoningImmune = true,
            HeadType = HeadType.None,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Swimming,
            EyeCount = 0,
            Mindless = true,
            NonAnimal = true
        };
    }
}
