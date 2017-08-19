using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant BarbedDevil = new CreatureVariant
        {
            Name = "barbed devil",
            Species = Species.DemonMajor,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 100,
            Weight = 1200,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Sting,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 7}}
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
                    {"thick hide", 3},
                    {"magic resistance", 35}
                },
            InitialLevel = 8,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight {Multiplier = 2F}, Name = "hell"},
            PreviousStageName = "horned devil",
            NextStageName = "nalfeshnee",
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.SmallGroup,
            Behavior = MonsterBehavior.Stalking
        };
    }
}