using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Hezrou = new CreatureVariant
        {
            Name = "hezrou",
            Species = Species.DemonMajor,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 200,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Poison {Damage = 3}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"infravision", "infravisibility", "humanoidness", "sickness resistance"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"poison resistance", 3},
                    {"size", 8},
                    {"physical deflection", 22},
                    {"magic resistance", 55},
                    {"weight", 1600}
                },
            InitialLevel = 7,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight {Multiplier = 2F}, Name = "hell"},
            NextStageName = "marilith",
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.SmallGroup,
            Behavior = MonsterBehavior.Stalking
        };
    }
}