using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant KoboldMummy = new CreatureVariant
        {
            Name = "kobold mummy",
            Species = Species.Kobold,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 200,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Infect()}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Poison {Damage = 50}}
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
                {
                    "cold resistance",
                    75
                },
                {
                    "poison resistance",
                    75
                },
                {
                    "size",
                    2
                },
                {
                    "physical deflection",
                    14
                },
                {
                    "magic resistance",
                    20
                },
                {
                    "weight",
                    400
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            CorpseName = "kobold",
            Noise = ActorNoiseType.Moan
        };
    }
}