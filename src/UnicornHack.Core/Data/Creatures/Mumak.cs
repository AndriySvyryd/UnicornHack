using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Mumak = new CreatureVariant
        {
            Name = "mumak",
            Species = Species.Quadruped,
            SpeciesClass = SpeciesClass.Quadrupedal,
            MovementDelay = 133,
            Weight = 2500,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Headbutt,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 13}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 7}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties = new Dictionary<string, object> {{"thick hide", 3}, {"largeness", Size.Large}},
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Noise = ActorNoiseType.Roar
        };
    }
}