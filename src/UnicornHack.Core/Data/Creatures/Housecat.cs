using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Housecat = new CreatureVariant
        {
            Name = "housecat",
            Species = Species.Cat,
            SpeciesClass = SpeciesClass.Feline,
            MovementDelay = 75,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 3}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties =
                new Dictionary<string, object> {{"size", 2}, {"physical deflection", 15}, {"weight", 200}},
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 7F},
            PreviousStageName = "kitten",
            NextStageName = "large cat",
            Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Wandering,
            Noise = ActorNoiseType.Mew
        };
    }
}