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
            MovementDelay = 60,
            Material = Material.Air,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Touch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Engulf {Duration = 4}}
                },
                new Ability
                {
                    Activation = ActivationType.OnDigestion,
                    Action = AbilityAction.Digestion,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Freeze {Damage = 30}}
                },
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeHit,
                    Effects = new HashSet<Effect> {new Freeze {Damage = 30}}
                },
                new Ability
                {
                    Activation = ActivationType.OnPhysicalRangedHit,
                    Effects = new HashSet<Effect> {new Freeze {Damage = 30}}
                }
            },
            InitialLevel = 5,
            GenerationWeight = new BranchWeight {NotMatched = new DefaultWeight {Multiplier = 5F}, Name = "hell"},
            Sex = Sex.None,
            Size = 16,
            Weight = 0,
            Perception = 3,
            Might = 2,
            Speed = 3,
            Focus = 8,
            MagicDeflection = 15,
            PhysicalDeflection = 18,
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
