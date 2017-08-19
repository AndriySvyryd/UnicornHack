using System.Collections.Generic;
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
            Weight = 1000,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Spell,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new MagicalDamage {Damage = 4}}
                    }
                },
            SimpleProperties = new HashSet<string> {"infravisibility", "humanoidness", "femaleness"},
            ValuedProperties = new Dictionary<string, object> {{"magic resistance", 50}},
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.Peaceful,
            Noise = ActorNoiseType.Oracle
        };
    }
}