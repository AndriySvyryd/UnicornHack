using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant RabidRat = new CreatureVariant
        {
            Name = "rabid rat",
            Species = Species.Rat,
            SpeciesClass = SpeciesClass.Rodent,
            MovementDelay = 100,
            Weight = 150,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect>
                        {
                            new ChangeProperty<int> {PropertyName = "Constitution", Value = -1, Duration = 5}
                        }
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 3}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties = new Dictionary<string, object> {{"largeness", Size.Small}, {"physical deflection", 14}},
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Sqeek
        };
    }
}