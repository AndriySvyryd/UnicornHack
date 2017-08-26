using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant UmberHulk = new CreatureVariant
        {
            Name = "umber hulk",
            Species = Species.Hulk,
            SpeciesClass = SpeciesClass.MagicalBeast,
            MovementDelay = 200,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 7}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 7}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 6}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Gaze,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Confuse {Duration = 4}}
                    }
                },
            SimpleProperties = new HashSet<string> {"tunneling", "animal body", "infravision", "infravisibility"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"thick hide", 3},
                    {"size", 8},
                    {"physical deflection", 18},
                    {"magic resistance", 25},
                    {"weight", 1300}
                },
            InitialLevel = 9,
            GenerationWeight = new DefaultWeight {Multiplier = 2F}
        };
    }
}