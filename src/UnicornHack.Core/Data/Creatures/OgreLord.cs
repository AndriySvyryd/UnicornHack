using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant OgreLord = new CreatureVariant
        {
            Name = "ogre lord",
            Species = Species.Ogre,
            MovementDelay = 100,
            Weight = 1650,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 7}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    }
                },
            SimpleProperties = new HashSet<string> {"infravision", "infravisibility", "humanoidness", "maleness"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"largeness", Size.Large},
                    {"physical deflection", 17},
                    {"magic resistance", 30}
                },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            PreviousStageName = "ogre",
            NextStageName = "ogre king",
            Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Grunt
        };
    }
}