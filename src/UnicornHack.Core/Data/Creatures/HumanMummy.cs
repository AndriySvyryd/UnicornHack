using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant HumanMummy = new CreatureVariant
        {
            Name = "human mummy",
            Species = Species.Human,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 100,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Infect()}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "infravision",
                "humanoidness",
                "breathlessness",
                "sickness resistance"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {"cold resistance", 75},
                {"poison resistance", 75},
                {"physical deflection", 16},
                {"magic resistance", 20},
                {"weight", 1000}
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            CorpseName = "human",
            Noise = ActorNoiseType.Moan
        };
    }
}