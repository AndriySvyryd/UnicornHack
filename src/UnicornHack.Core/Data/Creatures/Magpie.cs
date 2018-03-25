using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Magpie = new Creature
        {
            Name = "magpie",
            Species = Species.Crow,
            SpeciesClass = SpeciesClass.Bird,
            MovementDelay = 60,
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
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Behavior = AIBehavior.Wandering | AIBehavior.GemCollector,
            Noise = ActorNoiseType.Squawk,
            Size = 1,
            Weight = 50,
            Agility = 2,
            Constitution = 2,
            Intelligence = 2,
            Quickness = 2,
            Strength = 2,
            Willpower = 2,
            PhysicalDeflection = 14,
            TorsoType = TorsoType.Quadruped,
            UpperExtremeties = ExtremityType.None,
            LowerExtremeties = ExtremityType.Claws,
            LocomotionType = LocomotionType.Flying,
            InventorySize = 1,
            Infravisible = true
        };
    }
}
