using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature EnergyVortex = new Creature
        {
            Name = "energy vortex",
            Species = Species.Vortex,
            SpeciesClass = SpeciesClass.Extraplanar,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Touch,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect>
                        {new Engulf {Duration = EffectDuration.UntilTimeout, DurationAmount = "4"}}
                },
                new Ability
                {
                    Activation = ActivationType.OnDigestion,
                    Action = AbilityAction.Digestion,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Shock {Damage = "30"}}
                },
                new Ability
                {
                    Activation = ActivationType.OnDigestion,
                    Action = AbilityAction.Digestion,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new DrainEnergy {Amount = "10*mentalScaling"}}
                },
                new Ability
                {
                    Activation = ActivationType.OnMeleeHit,
                    Action = AbilityAction.Touch,
                    SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                    Accuracy = "10+attackScaling",
                    Effects = new HashSet<Effect> {new Shock {Damage = "30*mentalScaling"}}
                },
                new Ability
                {
                    Activation = ActivationType.OnRangedHit,
                    Action = AbilityAction.Shoot,
                    SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                    Accuracy = "10+attackScaling",
                    Effects = new HashSet<Effect> {new Shock {Damage = "30*mentalScaling"}}
                }
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            Sex = Sex.None,
            Size = 16,
            Weight = 0,
            MovementDelay = -40,
            TurningDelay = -40,
            Material = Material.Air,
            Perception = -6,
            Might = -6,
            Speed = -6,
            Focus = -2,
            Armor = 4,
            MagicResistance = 15,
            ElectricityResistance = 75,
            VoidResistance = 75,
            SlimingImmune = true,
            StoningImmune = true,
            HeadType = HeadType.None,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Flying,
            EyeCount = 0,
            Mindless = true,
            NonAnimal = true
        };
    }
}
