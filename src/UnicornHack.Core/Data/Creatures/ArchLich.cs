using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant ArchLich = new CreatureVariant
        {
            Name = "arch-lich",
            Species = Species.Lich,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 133,
            Weight = 600,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new ColdDamage {Damage = 17}}
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
                        Effects = new HashSet<Effect> {new Infect()}
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
                    "sleep resistance",
                    "infravision",
                    "humanoidness",
                    "breathlessness",
                    "sickness resistance"
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
                    {"magic resistance", 90}
                },
            InitialLevel = 25,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight(), Name = "hell"},
            PreviousStageName = "master lich",
            CorpseName = "",
            Behavior = MonsterBehavior.MagicUser | MonsterBehavior.Covetous,
            Noise = ActorNoiseType.Mumble
        };
    }
}