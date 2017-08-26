using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant LargeCat = new CreatureVariant
        {
            Name = "large cat",
            Species = Species.Cat,
            SpeciesClass = SpeciesClass.Feline,
            MovementDelay = 80,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties =
                new Dictionary<string, object> {{"size", 2}, {"physical deflection", 16}, {"weight", 250}},
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 7F},
            PreviousStageName = "housecat",
            Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Wandering,
            Noise = ActorNoiseType.Bark
        };
    }
}