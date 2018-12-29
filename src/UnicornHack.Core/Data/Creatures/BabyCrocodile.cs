using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature BabyCrocodile = new Creature
        {
            Name = "baby crocodile",
            Species = Species.Crocodile,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 200,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 60}}
                }
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            NextStageName = "crocodile",
            Size = 2,
            Weight = 200,
            Perception = 4,
            Might = 4,
            Speed = 4,
            Focus = 4,
            PhysicalDeflection = 14,
            UpperExtremities = ExtremityType.None,
            RespirationType = RespirationType.Amphibious,
            LocomotionType = LocomotionType.Swimming,
            InventorySize = 1
        };
    }
}
