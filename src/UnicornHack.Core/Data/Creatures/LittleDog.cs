using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant LittleDog = new CreatureVariant
        {
            Name = "little dog",
            Species = Species.Dog,
            SpeciesClass = SpeciesClass.Canine,
            MovementDelay = 66,
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
                new Dictionary<string, object> {{"size", 2}, {"physical deflection", 14}, {"weight", 150}},
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 7F},
            NextStageName = "dog",
            Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Wandering,
            Noise = ActorNoiseType.Bark
        };
    }
}