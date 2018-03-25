using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Warhorse = new Creature
        {
            Name = "warhorse",
            Species = Species.Horse,
            SpeciesClass = SpeciesClass.Quadrupedal,
            MovementDelay = 50,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Kick,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
                }
            },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 7F},
            PreviousStageName = "horse",
            Behavior = AIBehavior.Domesticable | AIBehavior.Mountable | AIBehavior.Wandering,
            Noise = ActorNoiseType.Neigh,
            Size = 8,
            Weight = 1800,
            Agility = 4,
            Constitution = 4,
            Intelligence = 4,
            Quickness = 4,
            Strength = 4,
            Willpower = 4,
            PhysicalDeflection = 16,
            TorsoType = TorsoType.Quadruped,
            UpperExtremeties = ExtremityType.None,
            LowerExtremeties = ExtremityType.Claws,
            InventorySize = 1,
            Infravisible = true
        };
    }
}
