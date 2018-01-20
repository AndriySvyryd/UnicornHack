using System.Collections.Generic;
using UnicornHack.Abilities;
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
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 20}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Stone()}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeHit,
                    Effects = new HashSet<Effect> {new Stone()}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Stone()}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "animal body",
                "infravisibility",
                "handlessness",
                "oviparity",
                "singular inventory",
                "stoning resistance"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 75},
                {"size", 2},
                {"physical deflection", 14},
                {"magic resistance", 30},
                {"weight", 30}
            },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            PreviousStageName = "chickatrice",
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Hiss
        };
    }
}