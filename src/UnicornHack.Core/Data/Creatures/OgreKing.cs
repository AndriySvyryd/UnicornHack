using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant OgreKing = new CreatureVariant
        {
            Name = "ogre king",
            Species = Species.Ogre,
            MovementDelay = 85,
            Weight = 1700,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 9}}
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
                    {"physical deflection", 16},
                    {"magic resistance", 60}
                },
            InitialLevel = 9,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            PreviousStageName = "ogre lord",
            GenerationFlags = GenerationFlags.Entourage,
            Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Grunt
        };
    }
}