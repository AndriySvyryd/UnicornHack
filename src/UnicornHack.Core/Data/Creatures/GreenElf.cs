using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GreenElf = new CreatureVariant
        {
            Name = "green-elf",
            Species = Species.Elf,
            MovementDelay = 100,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "invisibility detection",
                "infravision",
                "infravisibility",
                "humanoidness"
            },
            ValuedProperties =
                new Dictionary<string, object> {{"physical deflection", 10}, {"magic resistance", 10}, {"weight", 800}},
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            GenerationFlags = GenerationFlags.SmallGroup,
            Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Speach
        };
    }
}