using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature BabyLongWorm = new Creature
        {
            Name = "baby long worm",
            Species = Species.Worm,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 400,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 20}}
                }
            },
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            NextStageName = "long worm",
            Weight = 600,
            Perception = 2,
            Might = 2,
            Speed = 2,
            Focus = 2,
            PhysicalDeflection = 15,
            TorsoType = TorsoType.Serpentine,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            InventorySize = 0,
            EyeCount = 0,
            NoiseLevel = 0
        };
    }
}
