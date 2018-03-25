using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Hulker = new Creature
        {
            Name = "hulker",
            Species = Species.Hulk,
            SpeciesClass = SpeciesClass.MagicalBeast,
            MovementDelay = 200,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Claw,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Claw,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 60}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Gaze,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Confuse {Duration = 4}}
                }
            },
            InitialLevel = 9,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            Size = 8,
            Weight = 1300,
            Agility = 5,
            Constitution = 5,
            Intelligence = 5,
            Quickness = 5,
            Strength = 5,
            Willpower = 5,
            MagicResistance = 25,
            PhysicalDeflection = 18,
            TorsoType = TorsoType.Quadruped,
            UpperExtremeties = ExtremityType.Claws,
            LowerExtremeties = ExtremityType.Claws,
            LocomotionType = LocomotionType.Walking | LocomotionType.Tunneling,
            Infravisible = true,
            Infravision = true
        };
    }
}
