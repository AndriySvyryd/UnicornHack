using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant SewerRat = new CreatureVariant
        {
            Name = "sewer rat",
            Species = Species.Rat,
            SpeciesClass = SpeciesClass.Rodent,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties =
                new Dictionary<string, object> {{"size", 2}, {"physical deflection", 13}, {"weight", 100}},
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 7F},
            NextStageName = "giant rat",
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Sqeek
        };
    }
}