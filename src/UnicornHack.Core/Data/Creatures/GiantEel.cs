using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GiantEel = new CreatureVariant
        {
            Name = "giant eel",
            Species = Species.Eel,
            MovementDelay = 133,
            Weight = 600,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Hug,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Bind {Duration = 7}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"swimming", "water breathing", "limblessness", "oviparity", "no inventory"},
            ValuedProperties = new Dictionary<string, object> {{"largeness", Size.Large}, {"physical deflection", 21}},
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            NextStageName = "electric eel"
        };
    }
}