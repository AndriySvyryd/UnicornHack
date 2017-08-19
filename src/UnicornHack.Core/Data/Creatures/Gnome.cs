using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Gnome = new CreatureVariant
        {
            Name = "gnome",
            Species = Species.Gnome,
            MovementDelay = 200,
            Weight = 650,
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
            SimpleProperties = new HashSet<string> {"infravision", "infravisibility", "humanoidness"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"largeness", Size.Small},
                    {"physical deflection", 10},
                    {"magic resistance", 5}
                },
            InitialLevel = 1,
            NextStageName = "gnome lord",
            GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.SmallGroup,
            Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Speach
        };
    }
}