using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Asmodeus = new CreatureVariant
        {
            Name = "Asmodeus",
            Species = Species.DemonMajor,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 100,
            Weight = 1500,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new VenomDamage {Damage = 10}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Spell,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new ColdDamage {Damage = 21}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 18}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "flight",
                    "flight control",
                    "infravisibility",
                    "infravision",
                    "invisibility detection",
                    "humanoidness",
                    "maleness",
                    "sickness resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"cold resistance", 3},
                    {"poison resistance", 3},
                    {"largeness", Size.Large},
                    {"physical deflection", 27},
                    {"magic resistance", 90}
                },
            InitialLevel = 30,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight {Multiplier = 0F}, Name = "hell"},
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.Covetous |
                       MonsterBehavior.Bribeable,
            Noise = ActorNoiseType.Bribe
        };
    }
}