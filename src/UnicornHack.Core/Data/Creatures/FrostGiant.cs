using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant FrostGiant = new CreatureVariant
        {
            Name = "frost giant",
            Species = Species.Giant,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 11}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    }
                },
            SimpleProperties = new HashSet<string> {"infravision", "infravisibility", "humanoidness"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"cold resistance", 3},
                    {"size", 16},
                    {"physical deflection", 17},
                    {"magic resistance", 10},
                    {"weight", 2250}
                },
            InitialLevel = 10,
            GenerationWeight = new BranchWeight {NotMatched = new DefaultWeight {Multiplier = 2F}, Name = "hell"},
            GenerationFlags = GenerationFlags.SmallGroup,
            Behavior = MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Boast
        };
    }
}