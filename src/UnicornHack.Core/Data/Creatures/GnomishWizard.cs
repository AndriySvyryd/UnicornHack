using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GnomishWizard = new CreatureVariant
        {
            Name = "gnomish wizard",
            Species = Species.Gnome,
            MovementDelay = 150,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Spell,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new ScriptedEffect {Script = "ArcaneSpell"}}
                    }
                },
            SimpleProperties = new HashSet<string> {"infravision", "infravisibility", "humanoidness", "maleness"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"size", 2},
                    {"physical deflection", 10},
                    {"magic resistance", 20},
                    {"weight", 700}
                },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser,
            Noise = ActorNoiseType.Speach
        };
    }
}