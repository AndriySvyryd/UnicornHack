using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant PurpleDragon = new CreatureVariant
        {
            Name = "purple dragon",
            Species = Species.Dragon,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 133,
            Weight = 4500,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Breath,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new VenomDamage {Damage = 14}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 9}}
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
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "flight",
                    "flight control",
                    "invisibility detection",
                    "infravision",
                    "animal body",
                    "handlessness",
                    "oviparity",
                    "singular inventory"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"venom resistance", 3},
                    {"poison resistance", 3},
                    {"danger awareness", 3},
                    {"thick hide", 3},
                    {"largeness", Size.Gigantic},
                    {"physical deflection", 21},
                    {"magic resistance", 20}
                },
            InitialLevel = 15,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            PreviousStageName = "baby purple dragon",
            Behavior = MonsterBehavior.Mountable | MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector,
            Noise = ActorNoiseType.Roar
        };
    }
}