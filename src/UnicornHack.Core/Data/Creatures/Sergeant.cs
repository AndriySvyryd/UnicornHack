using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Sergeant = new CreatureVariant
        {
            Name = "sergeant",
            Species = Species.Human,
            MovementDelay = 120,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 7}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    }
                },
            SimpleProperties = new HashSet<string> {"infravisibility", "humanoidness"},
            ValuedProperties =
                new Dictionary<string, object> {{"physical deflection", 10}, {"magic resistance", 5}, {"weight", 1000}},
            InitialLevel = 8,
            PreviousStageName = "soldier",
            NextStageName = "lieutenant",
            GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.SmallGroup,
            Behavior = MonsterBehavior.Wandering | MonsterBehavior.Stalking | MonsterBehavior.Displacing |
                       MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector | MonsterBehavior.Bribeable,
            Noise = ActorNoiseType.Soldier
        };
    }
}