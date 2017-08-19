using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant KoboldShaman = new CreatureVariant
        {
            Name = "kobold shaman",
            Species = Species.Kobold,
            MovementDelay = 200,
            Weight = 450,
            Abilities =
                new HashSet<Ability>
                {
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
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 6}}
                    }
                },
            SimpleProperties = new HashSet<string> {"infravision", "infravisibility", "humanoidness", "maleness"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"largeness", Size.Small},
                    {"physical deflection", 12},
                    {"magic resistance", 10}
                },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Behavior = MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser,
            Noise = ActorNoiseType.Grunt
        };
    }
}