using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Jellyfish = new CreatureVariant
        {
            Name = "jellyfish",
            Species = Species.Jellyfish,
            MovementDelay = 400,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Sting,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Envenom {Damage = 6}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Poison {Damage = 2}}
                    }
                },
            SimpleProperties = new HashSet<string> {"swimming", "water breathing", "limblessness", "no inventory"},
            ValuedProperties =
                new Dictionary<string, object> {{"size", 2}, {"physical deflection", 14}, {"weight", 80}},
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 4F}
        };
    }
}