using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant AntQueen = new CreatureVariant
        {
            Name = "ant queen",
            Species = Species.Ant,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 66,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Poison {Damage = 50}}
                }
            },
            SimpleProperties = new HashSet<string> {"animal body", "handlessness", "femaleness", "oviparity"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 75},
                {"stealthiness", 3},
                {"size", 1},
                {"magic resistance", 20},
                {"weight", 10}
            },
            InitialLevel = 9,
            GenerationFlags = GenerationFlags.Entourage
        };
    }
}