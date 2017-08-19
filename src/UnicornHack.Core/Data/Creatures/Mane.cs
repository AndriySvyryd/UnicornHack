using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Mane = new CreatureVariant
        {
            Name = "mane",
            Species = Species.Homunculus,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 400,
            Weight = 500,
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
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 2}}
                    }
                },
            SimpleProperties = new HashSet<string> {"sleep resistance", "infravision", "infravisibility"},
            ValuedProperties = new Dictionary<string, object> {{"poison resistance", 3}, {"physical deflection", 13}},
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            CorpseName = "",
            GenerationFlags = GenerationFlags.LargeGroup,
            Behavior = MonsterBehavior.Stalking,
            Noise = ActorNoiseType.Hiss
        };
    }
}