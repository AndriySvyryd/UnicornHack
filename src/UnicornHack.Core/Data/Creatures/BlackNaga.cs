using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature BlackNaga = new Creature
        {
            Name = "black naga",
            Species = Species.Naga,
            SpeciesClass = SpeciesClass.Aberration,
            MovementDelay = 85,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Spit,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Corrode {Damage = 70}}
                }
            },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            PreviousStageName = "black naga hatchling",
            Noise = ActorNoiseType.Hiss,
            Size = 16,
            Weight = 1500,
            Perception = 5,
            Might = 4,
            Speed = 5,
            Focus = 4,
            MagicDeflection = 5,
            PhysicalDeflection = 18,
            AcidResistance = 75,
            StoningImmune = true,
            TorsoType = TorsoType.Serpentine,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            InventorySize = 1,
            Infravision = true
        };
    }
}
