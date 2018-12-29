using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature RustMonster = new Creature
        {
            Name = "rust monster",
            Species = Species.RustMonster,
            MovementDelay = 66,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Touch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Soak {Damage = 70}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Touch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Soak {Damage = 70}}
                },
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeHit,
                    Effects = new HashSet<Effect> {new Soak {Damage = 100}}
                }
            },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Weight = 1000,
            Perception = 3,
            Might = 2,
            Speed = 3,
            Focus = 2,
            PhysicalDeflection = 18,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            LocomotionType = LocomotionType.Swimming,
            InventorySize = 1,
            Infravisible = true
        };
    }
}
