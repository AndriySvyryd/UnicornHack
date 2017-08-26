using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant DwarfKing = new CreatureVariant
        {
            Name = "dwarf king",
            Species = Species.Dwarf,
            MovementDelay = 200,
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
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"tool tunneling", "infravision", "infravisibility", "humanoidness", "maleness"},
            ValuedProperties =
                new Dictionary<string, object> {{"physical deflection", 10}, {"magic resistance", 20}, {"weight", 900}},
            InitialLevel = 6,
            PreviousStageName = "dwarf lord",
            GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.Entourage,
            Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector |
                       MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Speach
        };
    }
}