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
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Claw,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 60}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Gaze,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Confuse {Duration = 4}}
                }
            },
            InitialLevel = 9,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            Size = 8,
            Weight = 1300,
            Perception = 5,
            Might = 4,
            Speed = 5,
            Focus = 4,
            MagicDeflection = 12,
            PhysicalDeflection = 18,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.Claws,
            LowerExtremities = ExtremityType.Claws,
            LocomotionType = LocomotionType.Walking | LocomotionType.Tunneling,
            Infravisible = true,
            Infravision = true
        };
    }
}
