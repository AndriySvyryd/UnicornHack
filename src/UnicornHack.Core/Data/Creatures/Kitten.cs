using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Kitten = new CreatureVariant
        {
            Name = "kitten",
            Species = Species.Cat,
            SpeciesClass = SpeciesClass.Feline,
            MovementDelay = 66,
            Weight = 150,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 3}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties = new Dictionary<string, object> {{"largeness", Size.Small}, {"physical deflection", 14}},
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 7F},
            NextStageName = "housecat",
            Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Wandering,
            Noise = ActorNoiseType.Mew
        };
    }
}