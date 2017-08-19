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
            Weight = 80,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Sting,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new VenomDamage {Damage = 6}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 2}}
                    }
                },
            SimpleProperties = new HashSet<string> {"swimming", "water breathing", "limblessness", "no inventory"},
            ValuedProperties = new Dictionary<string, object> {{"largeness", Size.Small}, {"physical deflection", 14}},
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 4F}
        };
    }
}