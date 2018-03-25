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
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Gaze,
                    Timeout = 7,
                    Effects = new HashSet<Effect> {new Disintegrate {Damage = 50}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Gaze,
                    Timeout = 7,
                    Effects = new HashSet<Effect> {new Slow {Duration = 13}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Gaze,
                    Timeout = 7,
                    Effects = new HashSet<Effect> {new Sedate {Duration = 13}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Gaze,
                    Timeout = 7,
                    Effects = new HashSet<Effect> {new Confuse {Duration = 13}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Gaze,
                    Timeout = 7,
                    Effects = new HashSet<Effect>
                    {
                        new Stone()
                    }
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Gaze,
                    Timeout = 7,
                    Effects = new HashSet<Effect> {new DrainEnergy {Amount = 3}}
                }
            },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.Wandering,
            Sex = Sex.None,
            Weight = 250,
            Agility = 5,
            Constitution = 5,
            Intelligence = 5,
            Quickness = 5,
            Strength = 5,
            Willpower = 5,
            MagicResistance = 35,
            PhysicalDeflection = 16,
            ColdResistance = 75,
            HeadType = HeadType.None,
            UpperExtremeties = ExtremityType.None,
            LowerExtremeties = ExtremityType.None,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Flying,
            InventorySize = 0,
            NoiseLevel = 0,
            Infravisible = true,
            Infravision = true
        };
    }
}
