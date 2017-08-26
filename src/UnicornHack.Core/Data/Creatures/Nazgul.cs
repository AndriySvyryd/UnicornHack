using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Nazgul = new CreatureVariant
        {
            Name = "nazgul",
            Species = Species.Wraith,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new DrainLife {Amount = 1}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Breath,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Sedate {Duration = 26}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new DrainLife {Amount = 1}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "flight",
                    "flight control",
                    "infravision",
                    "non solid body",
                    "humanoidness",
                    "breathlessness",
                    "maleness",
                    "stoning resistance",
                    "sliming resistance",
                    "sickness resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"cold resistance", 3},
                    {"poison resistance", 3},
                    {"magic resistance", 25},
                    {"weight", 1000}
                },
            InitialLevel = 13,
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Howl
        };
    }
}