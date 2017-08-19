using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Quasit = new CreatureVariant
        {
            Name = "quasit",
            Species = Species.Imp,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 80,
            Weight = 200,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect>
                        {
                            new ChangeProperty<int> {PropertyName = "Agility", Value = -1, Duration = 5}
                        }
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 3}}
                    }
                },
            SimpleProperties = new HashSet<string> {"infravision", "infravisibility"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"regeneration", 3},
                    {"largeness", Size.Small},
                    {"physical deflection", 18},
                    {"magic resistance", 20}
                },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 7F},
            CorpseName = "",
            Behavior = MonsterBehavior.Wandering | MonsterBehavior.Stalking,
            Noise = ActorNoiseType.Cuss
        };
    }
}