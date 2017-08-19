using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GnomeLord = new CreatureVariant
        {
            Name = "gnome lord",
            Species = Species.Gnome,
            MovementDelay = 150,
            Weight = 700,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 4}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    }
                },
            SimpleProperties = new HashSet<string> {"infravision", "infravisibility", "humanoidness", "maleness"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"largeness", Size.Small},
                    {"physical deflection", 10},
                    {"magic resistance", 5}
                },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            PreviousStageName = "gnome",
            NextStageName = "gnome king",
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Speach
        };
    }
}