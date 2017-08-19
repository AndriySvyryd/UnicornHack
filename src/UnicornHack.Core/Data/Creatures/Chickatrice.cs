using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Chickatrice = new CreatureVariant
        {
            Name = "chickatrice",
            Species = Species.Cockatrice,
            SpeciesClass = SpeciesClass.MagicalBeast,
            MovementDelay = 300,
            Weight = 10,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 5,
                        Effects = new HashSet<Effect> {new Stone()}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new Stone()}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Stone()}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "animal body",
                    "infravisibility",
                    "handlessness",
                    "singular inventory",
                    "stoning resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"largeness", Size.Tiny},
                    {"physical deflection", 12},
                    {"magic resistance", 30}
                },
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            NextStageName = "cockatrice",
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Hiss
        };
    }
}