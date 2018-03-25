using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature GoldenNaga = new Creature
        {
            Name = "golden naga",
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
                    Action = AbilityAction.Spell,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new MagicalDamage {Damage = 140}}
                }
            },
            InitialLevel = 10,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            PreviousStageName = "golden naga hatchling",
            Noise = ActorNoiseType.Hiss,
            Size = 16,
            Weight = 1500,
            Agility = 6,
            Constitution = 6,
            Intelligence = 6,
            Quickness = 6,
            Strength = 6,
            Willpower = 6,
            MagicResistance = 70,
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
