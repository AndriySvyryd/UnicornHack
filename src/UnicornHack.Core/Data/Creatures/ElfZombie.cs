using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant ElfZombie = new CreatureVariant
        {
            Name = "elf zombie",
            Species = Species.Elf,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 100,
            Weight = 800,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Infect()}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "invisibility detection",
                    "infravision",
                    "humanoidness",
                    "breathlessness",
                    "mindlessness",
                    "sickness resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"cold resistance", 3},
                    {"poison resistance", 3},
                    {"physical deflection", 11},
                    {"magic resistance", 10}
                },
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            CorpseName = "elf",
            GenerationFlags = GenerationFlags.SmallGroup,
            Behavior = MonsterBehavior.Stalking,
            Noise = ActorNoiseType.Moan
        };
    }
}