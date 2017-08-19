using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant MasterKaen = new CreatureVariant
        {
            Name = "Master Kaen",
            Species = Species.Human,
            MovementDelay = 100,
            Weight = 1000,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 24}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Spell,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new ScriptedEffect {Script = "DivineSpell"}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "invisibility detection",
                    "infravisibility",
                    "humanoidness",
                    "maleness",
                    "stoning resistance"
                },
            ValuedProperties = new Dictionary<string, object> {{"poison resistance", 3}, {"magic resistance", 10}},
            InitialLevel = 16,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.MagicUser |
                       MonsterBehavior.Covetous,
            Noise = ActorNoiseType.Quest
        };
    }
}