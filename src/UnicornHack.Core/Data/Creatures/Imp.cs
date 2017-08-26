using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Imp = new CreatureVariant
        {
            Name = "imp",
            Species = Species.Imp,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Poison {Damage = 2}}
                    }
                },
            SimpleProperties = new HashSet<string> {"flight", "flight control", "infravision", "infravisibility"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"regeneration", 3},
                    {"size", 1},
                    {"physical deflection", 18},
                    {"magic resistance", 20},
                    {"weight", 100}
                },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 7F},
            CorpseName = "",
            Behavior = MonsterBehavior.Wandering | MonsterBehavior.Stalking,
            Noise = ActorNoiseType.Cuss
        };
    }
}