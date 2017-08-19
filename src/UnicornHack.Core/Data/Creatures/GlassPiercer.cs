using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GlassPiercer = new CreatureVariant
        {
            Name = "glass piercer",
            Species = Species.Piercer,
            MovementDelay = 1200,
            Weight = 400,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 14}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"camouflage", "eyelessness", "limblessness", "clinginess", "no inventory"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"acid resistance", 3},
                    {"stealthiness", 3},
                    {"largeness", Size.Small},
                    {"physical deflection", 19}
                },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 3F}
        };
    }
}