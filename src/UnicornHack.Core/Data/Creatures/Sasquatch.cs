using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Sasquatch = new CreatureVariant
        {
            Name = "sasquatch",
            Species = Species.Simian,
            MovementDelay = 80,
            Weight = 1550,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 3}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 3}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 4}}
                    }
                },
            SimpleProperties = new HashSet<string> {"animal body", "infravisibility", "humanoidness"},
            ValuedProperties = new Dictionary<string, object> {{"largeness", Size.Large}, {"physical deflection", 14}},
            InitialLevel = 7,
            Noise = ActorNoiseType.Growl
        };
    }
}