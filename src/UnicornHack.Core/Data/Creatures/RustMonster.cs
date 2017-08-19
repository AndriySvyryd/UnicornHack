using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant RustMonster = new CreatureVariant
        {
            Name = "rust monster",
            Species = Species.RustMonster,
            MovementDelay = 66,
            Weight = 1000,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new WaterDamage {Damage = 7}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new WaterDamage {Damage = 7}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new WaterDamage {Damage = 10}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "swimming",
                    "infravisibility",
                    "animal body",
                    "handlessness",
                    "singular inventory"
                },
            ValuedProperties = new Dictionary<string, object> {{"physical deflection", 18}},
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 3F}
        };
    }
}