using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Oracle = new CreatureVariant
        {
            Name = "Oracle",
            Species = Species.Human,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Spell,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new MagicalDamage {Damage = 40}}
                }
            },
            SimpleProperties = new HashSet<string> {"infravisibility", "humanoidness", "femaleness"},
            ValuedProperties = new Dictionary<string, object> {{"magic resistance", 50}, {"weight", 1000}},
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.Peaceful,
            Noise = ActorNoiseType.Oracle
        };
    }
}