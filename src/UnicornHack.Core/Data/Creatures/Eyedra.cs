using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Eyedra = new Creature
        {
            Name = "eyedra",
            Species = Species.FloatingSphere,
            SpeciesClass = SpeciesClass.Aberration,
            MovementDelay = 300,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Gaze,
                    Cooldown = 350,
                    Effects = new HashSet<Effect> {new Disintegrate {Damage = 50}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Gaze,
                    Cooldown = 350,
                    Effects = new HashSet<Effect> {new Slow {Duration = 13}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Gaze,
                    Cooldown = 350,
                    Effects = new HashSet<Effect> {new Sedate {Duration = 13}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Gaze,
                    Cooldown = 350,
                    Effects = new HashSet<Effect> {new Confuse {Duration = 13}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted, Action = AbilityAction.Gaze, Cooldown = 350, Effects =
                        new HashSet<Effect>
                        {
                            new Stone()
                        }
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Gaze,
                    Cooldown = 350,
                    Effects = new HashSet<Effect> {new DrainEnergy {Amount = 3}}
                }
            },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.Wandering,
            Sex = Sex.None,
            Weight = 250,
            Perception = 5,
            Might = 4,
            Speed = 5,
            Focus = 4,
            MagicDeflection = 17,
            PhysicalDeflection = 16,
            ColdResistance = 75,
            HeadType = HeadType.None,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Flying,
            InventorySize = 0,
            NoiseLevel = 0,
            Infravisible = true,
            Infravision = true
        };
    }
}
