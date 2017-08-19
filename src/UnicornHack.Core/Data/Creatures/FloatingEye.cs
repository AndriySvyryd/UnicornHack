using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant FloatingEye = new CreatureVariant
        {
            Name = "floating eye",
            Species = Species.FloatingSphere,
            SpeciesClass = SpeciesClass.Aberration,
            MovementDelay = 1200,
            Weight = 10,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new Paralyze {Duration = 35}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "flight",
                    "flight control",
                    "infravision",
                    "infravisibility",
                    "non animal",
                    "breathlessness",
                    "limblessness",
                    "headlessness",
                    "mindlessness",
                    "asexuality",
                    "no inventory"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"stealthiness", 3},
                    {"largeness", Size.Small},
                    {"physical deflection", 11},
                    {"magic resistance", 10}
                },
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Behavior = MonsterBehavior.Wandering
        };
    }
}