using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant IronPiercer = new CreatureVariant
        {
            Name = "iron piercer",
            Species = Species.Piercer,
            MovementDelay = 1200,
            Weight = 300,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"camouflage", "eyelessness", "limblessness", "clinginess", "no inventory"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"stealthiness", 3},
                    {"largeness", Size.Small},
                    {"physical deflection", 18}
                },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 3F}
        };
    }
}