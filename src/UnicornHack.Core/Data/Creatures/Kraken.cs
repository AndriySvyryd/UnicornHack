using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Kraken = new CreatureVariant
        {
            Name = "kraken",
            Species = Species.Squid,
            MovementDelay = 400,
            Weight = 2000,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 12}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Hug,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Bind {Duration = 7}}
                    }
                },
            SimpleProperties = new HashSet<string> {"swimming", "water breathing", "limblessness", "oviparity"},
            ValuedProperties = new Dictionary<string, object> {{"largeness", Size.Huge}, {"physical deflection", 14}},
            InitialLevel = 20,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonPolymorphable
        };
    }
}