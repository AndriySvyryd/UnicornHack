using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant WizardOfYendor = new CreatureVariant
        {
            Name = "Wizard of Yendor",
            Species = Species.Human,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 13}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Spell,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new ScriptedEffect {Script = "ArcaneSpell"}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "flight",
                    "flight control",
                    "teleportation",
                    "teleportation control",
                    "breathlessness",
                    "infravisibility",
                    "invisibility detection",
                    "humanoidness",
                    "maleness"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"poison resistance", 3},
                    {"regeneration", 3},
                    {"energy regeneration", 3},
                    {"telepathy", 3},
                    {"physical deflection", 28},
                    {"magic resistance", 100},
                    {"weight", 1000}
                },
            InitialLevel = 30,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.MagicUser | MonsterBehavior.Covetous,
            Noise = ActorNoiseType.Cuss
        };
    }
}