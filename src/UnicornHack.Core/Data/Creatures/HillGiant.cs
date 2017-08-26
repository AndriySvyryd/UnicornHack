using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant HillGiant = new CreatureVariant
        {
            Name = "hill giant",
            Species = Species.Giant,
            MovementDelay = 120,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 9}}
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
                new Dictionary<string, object> {{"size", 16}, {"physical deflection", 14}, {"weight", 2250}},
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            GenerationFlags = GenerationFlags.SmallGroup,
            Behavior = MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Boast
        };
    }
}