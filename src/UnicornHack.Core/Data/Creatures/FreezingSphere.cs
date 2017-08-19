using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant FreezingSphere = new CreatureVariant
        {
            Name = "freezing sphere",
            Species = Species.FloatingSphere,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 92,
            Weight = 10,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Explosion,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new ColdDamage {Damage = 14}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "flight",
                    "flight control",
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
                    {"cold resistance", 3},
                    {"largeness", Size.Small},
                    {"physical deflection", 16},
                    {"magic resistance", 10}
                },
            InitialLevel = 6,
            GenerationWeight = new BranchWeight {NotMatched = new DefaultWeight {Multiplier = 4F}, Name = "hell"},
            CorpseName = ""
        };
    }
}