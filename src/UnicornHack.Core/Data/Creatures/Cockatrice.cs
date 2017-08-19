using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Cockatrice = new CreatureVariant
        {
            Name = "cockatrice",
            Species = Species.Cockatrice,
            SpeciesClass = SpeciesClass.MagicalBeast,
            MovementDelay = 200,
            Weight = 30,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
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
                    "oviparity",
                    "singular inventory",
                    "stoning resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"largeness", Size.Small},
                    {"physical deflection", 14},
                    {"magic resistance", 30}
                },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            PreviousStageName = "chickatrice",
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Hiss
        };
    }
}