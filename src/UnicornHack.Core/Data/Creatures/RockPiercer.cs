using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant RockPiercer = new CreatureVariant
        {
            Name = "rock piercer",
            Species = Species.Piercer,
            MovementDelay = 1200,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 7}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"camouflage", "eyelessness", "limblessness", "clinginess", "no inventory"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"stealthiness", 3},
                    {"size", 2},
                    {"physical deflection", 17},
                    {"weight", 200}
                },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 3F}
        };
    }
}