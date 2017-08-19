using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Tengu = new CreatureVariant
        {
            Name = "tengu",
            Species = Species.Tengu,
            SpeciesClass = SpeciesClass.ShapeChanger,
            MovementDelay = 92,
            Weight = 300,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 4}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 2}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Teleport()}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"teleportation", "teleportation control", "infravisibility", "infravision"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"largeness", Size.Small},
                    {"physical deflection", 15},
                    {"magic resistance", 30}
                },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Behavior = MonsterBehavior.Stalking,
            Noise = ActorNoiseType.Squawk
        };
    }
}