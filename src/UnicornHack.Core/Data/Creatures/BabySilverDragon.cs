using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature BabySilverDragon = new Creature
        {
            Name = "baby silver dragon",
            Species = Species.Dragon,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 133,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                }
            },
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            NextStageName = "silver dragon",
            Noise = ActorNoiseType.Roar,
            Size = 8,
            Weight = 1500,
            Perception = 7,
            Might = 6,
            Speed = 7,
            Focus = 6,
            MagicDeflection = 5,
            PhysicalDeflection = 18,
            Reflective = true,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            LocomotionType = LocomotionType.Flying,
            InventorySize = 1,
            Infravisible = true
        };
    }
}
