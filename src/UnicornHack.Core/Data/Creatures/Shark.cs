using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Shark = new Creature
        {
            Name = "shark",
            Species = Species.Fish,
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 170}}
                }
            },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            Size = 8,
            Weight = 1000,
            Perception = 4,
            Might = 4,
            Speed = 4,
            Focus = 4,
            PhysicalDeflection = 18,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.Water,
            LocomotionType = LocomotionType.Swimming,
            InventorySize = 0
        };
    }
}
