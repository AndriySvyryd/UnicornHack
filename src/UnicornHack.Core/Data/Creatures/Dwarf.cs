using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Dwarf = new CreatureVariant
        {
            Name = "dwarf",
            Species = Species.Dwarf,
            MovementDelay = 200,
            Weight = 900,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 3}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    }
                },
            SimpleProperties = new HashSet<string> {"tool tunneling", "infravision", "infravisibility", "humanoidness"},
            ValuedProperties = new Dictionary<string, object> {{"physical deflection", 10}, {"magic resistance", 10}},
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            NextStageName = "dwarf lord",
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector |
                       MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Speach
        };
    }
}