using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GuardianNaga = new CreatureVariant
        {
            Name = "guardian naga",
            Species = Species.Naga,
            SpeciesClass = SpeciesClass.Aberration,
            MovementDelay = 75,
            Weight = 1500,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Spit,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new VenomDamage {Damage = 14}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "infravision",
                    "serpentlike body",
                    "limblessness",
                    "oviparity",
                    "singular inventory"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"thick hide", 3},
                    {"largeness", Size.Huge},
                    {"magic resistance", 50}
                },
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            PreviousStageName = "guardian naga hatchling",
            Noise = ActorNoiseType.Hiss
        };
    }
}