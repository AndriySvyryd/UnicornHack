using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Mane = new CreatureVariant
        {
            Name = "mane",
            Species = Species.Homunculus,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 400,
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
            SimpleProperties = new HashSet<string> {"sleep resistance", "infravision", "infravisibility"},
            ValuedProperties =
                new Dictionary<string, object> {{"poison resistance", 3}, {"physical deflection", 13}, {"weight", 500}},
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            CorpseName = "",
            GenerationFlags = GenerationFlags.LargeGroup,
            Behavior = MonsterBehavior.Stalking,
            Noise = ActorNoiseType.Hiss
        };
    }
}