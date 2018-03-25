using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Pony = new Creature
        {
            Name = "pony",
            Species = Species.Horse,
            SpeciesClass = SpeciesClass.Quadrupedal,
            MovementDelay = 75,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Kick,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 7F},
            NextStageName = "horse",
            Behavior = AIBehavior.Domesticable | AIBehavior.Mountable | AIBehavior.Wandering,
            Noise = ActorNoiseType.Neigh,
            Weight = 1300,
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
            InventorySize = 1,
            Infravisible = true
        };
    }
}
