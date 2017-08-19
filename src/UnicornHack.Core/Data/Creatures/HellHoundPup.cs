using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant HellHoundPup = new CreatureVariant
        {
            Name = "hell hound pup",
            Species = Species.Dog,
            SpeciesClass = SpeciesClass.Canine,
            MovementDelay = 100,
            Weight = 250,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 7}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Breath,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new FireDamage {Damage = 7}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"largeness", Size.Small},
                    {"physical deflection", 16},
                    {"magic resistance", 20}
                },
            InitialLevel = 7,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight {Multiplier = 4F}, Name = "hell"},
            NextStageName = "hell hound",
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Bark
        };
    }
}