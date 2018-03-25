using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature BlackNagaHatchling = new Creature
        {
            Name = "black naga hatchling",
            Species = Species.Naga,
            SpeciesClass = SpeciesClass.Aberration,
            MovementDelay = 120,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 20}}
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            NextStageName = "black naga",
            Noise = ActorNoiseType.Hiss,
            Weight = 500,
            Agility = 2,
            Constitution = 2,
            Intelligence = 2,
            Quickness = 2,
            Strength = 2,
            Willpower = 2,
            PhysicalDeflection = 14,
            AcidResistance = 75,
            StoningImmune = true,
            TorsoType = TorsoType.Serpentine,
            UpperExtremeties = ExtremityType.None,
            LowerExtremeties = ExtremityType.None,
            InventorySize = 1,
            Infravision = true
        };
    }
}
