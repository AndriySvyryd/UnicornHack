using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature IceVortex = new Creature
        {
            Name = "ice vortex",
            Species = Species.Vortex,
            SpeciesClass = SpeciesClass.Extraplanar,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Touch,
                    SuccessCondition = AbilitySuccessCondition.Attack,
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
                    Effects = new HashSet<Effect> {new Freeze {Damage = "30"}}
                },
                new Ability
                {
                    Activation = ActivationType.OnMeleeHit, Effects = new HashSet<Effect> {new Freeze {Damage = "30"}}
                },
                new Ability
                {
                    Activation = ActivationType.OnRangedHit, Effects = new HashSet<Effect> {new Freeze {Damage = "30"}}
                }
            },
            InitialLevel = 5,
            GenerationWeight = new BranchWeight {NotMatched = new DefaultWeight {Multiplier = 5F}, Name = "hell"},
            Sex = Sex.None,
            Size = 16,
            Weight = 0,
            MovementDelay = -40,
            TurningDelay = -40,
            Material = Material.Air,
            Perception = -7,
            Might = -8,
            Speed = -7,
            Focus = -2,
            Armor = 4,
            MagicResistance = 15,
            ColdResistance = 75,
            SlimingImmune = true,
            StoningImmune = true,
            HeadType = HeadType.None,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Flying,
            EyeCount = 0,
            Infravisible = true,
            Mindless = true,
            NonAnimal = true
        };
    }
}
