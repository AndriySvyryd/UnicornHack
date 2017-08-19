using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Elvenking = new CreatureVariant
        {
            Name = "Elvenking",
            Species = Species.Elf,
            MovementDelay = 100,
            Weight = 800,
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
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "invisibility detection",
                    "infravision",
                    "infravisibility",
                    "humanoidness",
                    "maleness"
                },
            ValuedProperties = new Dictionary<string, object> {{"physical deflection", 10}, {"magic resistance", 25}},
            InitialLevel = 9,
            GenerationFlags = GenerationFlags.Entourage,
            Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Speach
        };
    }
}