using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature GiantRat = new Creature
        {
            Name = "giant rat",
            Species = Species.Rat,
            SpeciesClass = SpeciesClass.Rodent,
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
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            PreviousStageName = "sewer rat",
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Sqeek,
            Size = 2,
            Weight = 150,
            Agility = 2,
            Constitution = 2,
            Intelligence = 2,
            Quickness = 2,
            Strength = 2,
            Willpower = 2,
            PhysicalDeflection = 13,
            TorsoType = TorsoType.Quadruped,
            UpperExtremeties = ExtremityType.None,
            LowerExtremeties = ExtremityType.Claws,
            InventorySize = 1,
            Infravisible = true
        };
    }
}
