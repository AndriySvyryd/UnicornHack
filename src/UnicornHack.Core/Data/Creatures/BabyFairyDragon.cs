using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant BabyFairyDragon = new CreatureVariant
        {
            Name = "baby fairy dragon",
            Species = Species.Dragon,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 133,
            Weight = 1500,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 7}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "flight",
                    "flight control",
                    "invisibility",
                    "infravision",
                    "animal body",
                    "handlessness",
                    "singular inventory"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"thick hide", 3},
                    {"largeness", Size.Large},
                    {"physical deflection", 18},
                    {"magic resistance", 10}
                },
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            NextStageName = "fairy dragon",
            Noise = ActorNoiseType.Roar
        };
    }
}