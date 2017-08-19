using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Werejackal = new CreatureVariant
        {
            Name = "werejackal",
            Species = Species.Human,
            SpeciesClass = SpeciesClass.ShapeChanger,
            MovementDelay = 100,
            Weight = 1000,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 5,
                        Effects = new HashSet<Effect> {new ConferLycanthropy {VariantName = "jackalwere"}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new ConferLycanthropy {VariantName = "jackalwere"}}
                    }
                },
            SimpleProperties = new HashSet<string> {"infravisibility", "humanoidness"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"regeneration", 3},
                    {"lycanthropy", "jackalwere"},
                    {"physical deflection", 10},
                    {"magic resistance", 10}
                },
            InitialLevel = 2,
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Lycanthrope
        };
    }
}