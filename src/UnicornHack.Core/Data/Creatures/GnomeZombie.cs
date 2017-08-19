using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GnomeZombie = new CreatureVariant
        {
            Name = "gnome zombie",
            Species = Species.Gnome,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 200,
            Weight = 650,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 3}}
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
                    {"largeness", Size.Small},
                    {"physical deflection", 11}
                },
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            CorpseName = "gnome",
            GenerationFlags = GenerationFlags.SmallGroup,
            Behavior = MonsterBehavior.Stalking,
            Noise = ActorNoiseType.Moan
        };
    }
}