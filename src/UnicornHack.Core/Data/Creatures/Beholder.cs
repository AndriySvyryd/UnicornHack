using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Beholder = new CreatureVariant
        {
            Name = "beholder",
            Species = Species.FloatingSphere,
            SpeciesClass = SpeciesClass.Aberration,
            MovementDelay = 300,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Gaze,
                    Timeout = 7,
                    Effects = new HashSet<Effect> {new Disintegrate {Damage = 50}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Gaze,
                    Timeout = 7,
                    Effects = new HashSet<Effect> {new Slow {Duration = 13}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Gaze,
                    Timeout = 7,
                    Effects = new HashSet<Effect> {new Sedate {Duration = 13}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Gaze,
                    Timeout = 7,
                    Effects = new HashSet<Effect> {new Confuse {Duration = 13}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Gaze,
                    Timeout = 7,
                    Effects = new HashSet<Effect> {new Stone()}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Gaze,
                    Timeout = 7,
                    Effects = new HashSet<Effect> {new DrainEnergy {Amount = 3}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "flight",
                "flight control",
                "infravision",
                "infravisibility",
                "breathlessness",
                "limblessness",
                "headlessness",
                "asexuality",
                "no inventory"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "cold resistance",
                    3
                },
                {
                    "danger awareness",
                    3
                },
                {
                    "stealthiness",
                    3
                },
                {
                    "physical deflection",
                    16
                },
                {
                    "magic resistance",
                    35
                },
                {
                    "weight",
                    250
                }
            },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.Wandering
        };
    }
}