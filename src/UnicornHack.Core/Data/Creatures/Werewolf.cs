using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Werewolf = new CreatureVariant
        {
            Name = "werewolf",
            Species = Species.Human,
            SpeciesClass = SpeciesClass.ShapeChanger,
            MovementDelay = 100,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
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
                    Action = AbilityAction.Bite,
                    Timeout = 5,
                    Effects = new HashSet<Effect> {new ConferLycanthropy {VariantName = "wolfwere"}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new ConferLycanthropy {VariantName = "wolfwere"}}
                }
            },
            SimpleProperties = new HashSet<string> {"infravisibility", "humanoidness"},
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "poison resistance",
                    75
                },
                {
                    "regeneration",
                    3
                },
                {
                    "lycanthropy",
                    "wolfwere"
                },
                {
                    "physical deflection",
                    10
                },
                {
                    "magic resistance",
                    20
                },
                {
                    "weight",
                    1000
                }
            },
            InitialLevel = 5,
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Lycanthrope
        };
    }
}