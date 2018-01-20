using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Healer = new CreatureVariant
        {
            Name = "healer",
            Species = Species.Human,
            MovementDelay = 100,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Spell,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new ScriptedEffect {Script = "DivineSpell"}}
                }
            },
            SimpleProperties = new HashSet<string> {"infravisibility", "humanoidness"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 75},
                {"physical deflection", 10},
                {"magic resistance", 10},
                {"weight", 1000}
            },
            InitialLevel = 10,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            PreviousStageName = "attendant",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.Peaceful | MonsterBehavior.GoldCollector | MonsterBehavior.MagicUser,
            Noise = ActorNoiseType.Speach
        };
    }
}