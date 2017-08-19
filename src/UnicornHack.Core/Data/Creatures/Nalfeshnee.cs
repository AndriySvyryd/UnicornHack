using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Nalfeshnee = new CreatureVariant
        {
            Name = "nalfeshnee",
            Species = Species.DemonMajor,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 133,
            Weight = 2500,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Spell,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new ScriptedEffect {Script = "ArcaneSpell"}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 4}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "infravision",
                    "invisibility detection",
                    "infravisibility",
                    "humanoidness",
                    "sickness resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"poison resistance", 3},
                    {"largeness", Size.Huge},
                    {"physical deflection", 24},
                    {"magic resistance", 65}
                },
            InitialLevel = 11,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight(), Name = "hell"},
            PreviousStageName = "barbed devil",
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonGenocidable,
            Behavior = MonsterBehavior.Stalking | MonsterBehavior.MagicUser,
            Noise = ActorNoiseType.Cast
        };
    }
}