using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Aleax = new CreatureVariant
        {
            Name = "aleax",
            Species = Species.Angel,
            SpeciesClass = SpeciesClass.Celestial,
            MovementDelay = 150,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "flight",
                "flight control",
                "infravisibility",
                "infravision",
                "invisibility detection",
                "humanoidness"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "cold resistance",
                    75
                },
                {
                    "electricity resistance",
                    75
                },
                {
                    "poison resistance",
                    75
                },
                {
                    "venom resistance",
                    75
                },
                {
                    "magic resistance",
                    30
                },
                {
                    "weight",
                    1000
                }
            },
            InitialLevel = 10,
            GenerationWeight = new BranchWeight {NotMatched = new DefaultWeight {Multiplier = 4F}, Name = "hell"},
            NextStageName = "angel",
            CorpseName = "",
            Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Speach
        };
    }
}