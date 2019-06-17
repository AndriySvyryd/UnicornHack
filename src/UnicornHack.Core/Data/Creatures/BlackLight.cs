using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature BlackLight = new Creature
        {
            Name = "black light",
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
                    Effects = new HashSet<Effect> {new Confuse {DurationAmount = "27"}}
                }
            },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Sex = Sex.None,
            Size = 2,
            Weight = 0,
            MovementDelay = -20,
            TurningDelay = -20,
            Material = Material.Air,
            Perception = -7,
            Might = -8,
            Speed = -7,
            Focus = -2,
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
