using System.Collections.Generic;
using UnicornHack.Abilities;
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
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Infect()}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "infravision",
                "humanoidness",
                "breathlessness",
                "mindlessness",
                "sickness resistance"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {"cold resistance", 75},
                {"poison resistance", 75},
                {"size", 2},
                {"physical deflection", 11},
                {"weight", 650}
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