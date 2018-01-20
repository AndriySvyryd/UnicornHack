using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant HumanZombie = new CreatureVariant
        {
            Name = "human zombie",
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
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 40}}
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
                "mindlessness",
                "sickness resistance"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {"cold resistance", 75},
                {"poison resistance", 75},
                {"physical deflection", 11},
                {"weight", 1000}
            },
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            CorpseName = "human",
            GenerationFlags = GenerationFlags.SmallGroup,
            Behavior = MonsterBehavior.Stalking,
            Noise = ActorNoiseType.Moan
        };
    }
}