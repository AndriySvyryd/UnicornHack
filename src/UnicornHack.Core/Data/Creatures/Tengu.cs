using System.Collections.Generic;
using UnicornHack.Abilities;
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
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 40}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Poison {Damage = 20}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Teleport()}
                }
            },
            SimpleProperties =
                new HashSet<string> {"teleportation", "teleportation control", "infravisibility", "infravision"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 75},
                {"size", 2},
                {"physical deflection", 15},
                {"magic resistance", 30},
                {"weight", 300}
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Behavior = MonsterBehavior.Stalking,
            Noise = ActorNoiseType.Squawk
        };
    }
}