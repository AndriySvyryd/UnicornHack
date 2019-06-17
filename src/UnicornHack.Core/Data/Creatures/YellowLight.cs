using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature YellowLight = new Creature
        {
            Name = "yellow light",
            Species = Species.FloatingSphere,
            SpeciesClass = SpeciesClass.Extraplanar,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnRangedAttack,
                    Range = 20,
                    Action = AbilityAction.Explosion,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect>
                        {new Blind {Duration = EffectDuration.UntilTimeout, DurationAmount = "27"}}
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Sex = Sex.None,
            Size = 2,
            Weight = 0,
            MovementDelay = -20,
            TurningDelay = -20,
            Material = Material.Air,
            Perception = -8,
            Might = -8,
            Speed = -8,
            Focus = -4,
            AcidResistance = 75,
            ColdResistance = 75,
            ElectricityResistance = 75,
            FireResistance = 75,
            VoidResistance = 75,
            SlimingImmune = true,
            HeadType = HeadType.None,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Flying,
            InventorySize = 0,
            EyeCount = 0,
            NoiseLevel = 0,
            Infravisible = true,
            InvisibilityDetection = true,
            Mindless = true,
            NonAnimal = true
        };
    }
}
