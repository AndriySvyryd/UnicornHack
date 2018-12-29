using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Dingo = new Creature
        {
            Name = "dingo",
            Species = Species.Dog,
            SpeciesClass = SpeciesClass.Canine,
            MovementDelay = 75,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                }
            },
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            Noise = ActorNoiseType.Bark,
            Weight = 400,
            Perception = 3,
            Might = 2,
            Speed = 3,
            Focus = 2,
            PhysicalDeflection = 13,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            InventorySize = 1,
            Infravisible = true
        };
    }
}
