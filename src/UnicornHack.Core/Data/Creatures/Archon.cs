using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Archon = new CreatureVariant
        {
            Name = "archon",
            Species = Species.Angel,
            SpeciesClass = SpeciesClass.Celestial,
            MovementDelay = 75,
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
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Spell,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new MagicalDamage {Damage = 14}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "flight",
                    "flight control",
                    "infravisibility",
                    "infravision",
                    "invisibility detection",
                    "humanoidness"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"cold resistance", 3},
                    {"electricity resistance", 3},
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"regeneration", 3},
                    {"physical deflection", 26},
                    {"magic resistance", 80}
                },
            InitialLevel = 19,
            GenerationWeight = new BranchWeight {NotMatched = new DefaultWeight {Multiplier = 4F}, Name = "hell"},
            PreviousStageName = "angel",
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector |
                       MonsterBehavior.MagicUser,
            Noise = ActorNoiseType.Speach
        };
    }
}