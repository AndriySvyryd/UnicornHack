using System.Collections.Generic;
using UnicornHack.Abilities;
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
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 140}}
                }
            },
            SimpleProperties =
                new HashSet<string> {"camouflage", "eyelessness", "limblessness", "clinginess", "no inventory"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"acid resistance", 3},
                {"stealthiness", 3},
                {"size", 2},
                {"physical deflection", 19},
                {"weight", 400}
            },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 3F}
        };
    }
}