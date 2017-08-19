using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GoldenNaga = new CreatureVariant
        {
            Name = "golden naga",
            Species = Species.Naga,
            SpeciesClass = SpeciesClass.Aberration,
            MovementDelay = 85,
            Weight = 1500,
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
                        Action = AbilityAction.Spell,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new MagicalDamage {Damage = 14}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "infravision",
                    "serpentlike body",
                    "limblessness",
                    "oviparity",
                    "singular inventory",
                    "stoning resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"acid resistance", 3},
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"thick hide", 3},
                    {"largeness", Size.Huge},
                    {"physical deflection", 18},
                    {"magic resistance", 70}
                },
            InitialLevel = 10,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            PreviousStageName = "golden naga hatchling",
            Noise = ActorNoiseType.Hiss
        };
    }
}