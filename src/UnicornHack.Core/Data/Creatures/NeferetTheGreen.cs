using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant NeferetTheGreen = new CreatureVariant
        {
            Name = "Neferet the Green",
            Species = Species.Human,
            MovementDelay = 100,
            Weight = 1000,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Spell,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new ScriptedEffect {Script = "ArcaneSpell"}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"infravisibility", "humanoidness", "femaleness", "stoning resistance"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"physical deflection", 10},
                    {"magic resistance", 70}
                },
            InitialLevel = 16,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.Peaceful | MonsterBehavior.GoldCollector | MonsterBehavior.MagicUser,
            Noise = ActorNoiseType.Quest
        };
    }
}