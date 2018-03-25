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
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Spit,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Corrode {Damage = 70}}
                }
            },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            PreviousStageName = "black naga hatchling",
            Noise = ActorNoiseType.Hiss,
            Size = 16,
            Weight = 1500,
            Agility = 5,
            Constitution = 5,
            Intelligence = 5,
            Quickness = 5,
            Strength = 5,
            Willpower = 5,
            MagicResistance = 10,
            PhysicalDeflection = 18,
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
