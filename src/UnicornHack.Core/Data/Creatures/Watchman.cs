using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Watchman = new CreatureVariant
        {
            Name = "watchman",
            Species = Species.Human,
            MovementDelay = 120,
            Weight = 1000,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 4}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    }
                },
            SimpleProperties = new HashSet<string> {"infravisibility", "humanoidness"},
            ValuedProperties = new Dictionary<string, object> {{"physical deflection", 10}},
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            NextStageName = "watch captain",
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.Peaceful | MonsterBehavior.Wandering | MonsterBehavior.Stalking |
                       MonsterBehavior.Displacing | MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector |
                       MonsterBehavior.Bribeable,
            Noise = ActorNoiseType.Soldier
        };
    }
}