using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant BoneDevil = new CreatureVariant
        {
            Name = "bone devil",
            Species = Species.DemonMajor,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 80,
            Weight = 1600,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 6}}
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
                        Action = AbilityAction.Sting,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new VenomDamage {Damage = 5}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 3}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"infravision", "infravisibility", "humanoidness", "sickness resistance"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"poison resistance", 3},
                    {"largeness", Size.Large},
                    {"physical deflection", 21},
                    {"magic resistance", 40}
                },
            InitialLevel = 9,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight {Multiplier = 2F}, Name = "hell"},
            NextStageName = "ice devil",
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.SmallGroup,
            Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector
        };
    }
}