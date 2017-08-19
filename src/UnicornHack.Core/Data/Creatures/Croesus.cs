using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Croesus = new CreatureVariant
        {
            Name = "Croesus",
            Species = Species.Human,
            MovementDelay = 80,
            Weight = 1000,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 22}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"infravisibility", "invisibility detection", "humanoidness", "maleness"},
            ValuedProperties = new Dictionary<string, object> {{"magic resistance", 40}},
            InitialLevel = 20,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.Stalking | MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector |
                       MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser,
            Noise = ActorNoiseType.Guard
        };
    }
}