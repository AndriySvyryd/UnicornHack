using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant HellHound = new CreatureVariant
        {
            Name = "hell hound",
            Species = Species.Dog,
            SpeciesClass = SpeciesClass.Canine,
            MovementDelay = 85,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 100}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Breath,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Burn {Damage = 100}}
                }
            },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"fire resistance", 75},
                {"physical deflection", 18},
                {"magic resistance", 20},
                {"weight", 700}
            },
            InitialLevel = 12,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight {Multiplier = 6F}, Name = "hell"},
            PreviousStageName = "hell hound pup",
            Noise = ActorNoiseType.Bark
        };
    }
}