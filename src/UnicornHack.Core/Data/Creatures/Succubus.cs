using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Succubus = new CreatureVariant
        {
            Name = "succubus",
            Species = Species.Succubus,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Sedate {Duration = 5}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Poison {Damage = 2}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "flight",
                    "flight control",
                    "infravision",
                    "infravisibility",
                    "humanoidness",
                    "sickness resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"poison resistance", 3},
                    {"magic resistance", 70},
                    {"weight", 1400}
                },
            InitialLevel = 6,
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Seduction
        };
    }
}