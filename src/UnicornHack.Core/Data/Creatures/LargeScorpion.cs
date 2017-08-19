using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant LargeScorpion = new CreatureVariant
        {
            Name = "large scorpion",
            Species = Species.Scorpion,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 80,
            Weight = 150,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Sting,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new VenomDamage {Damage = 2}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 2}}
                    }
                },
            SimpleProperties = new HashSet<string> {"concealment", "animal body", "handlessness", "oviparity"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"largeness", Size.Small},
                    {"physical deflection", 17}
                },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 5F}
        };
    }
}