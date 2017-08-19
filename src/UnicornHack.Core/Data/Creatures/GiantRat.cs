using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GiantRat = new CreatureVariant
        {
            Name = "giant rat",
            Species = Species.Rat,
            SpeciesClass = SpeciesClass.Rodent,
            MovementDelay = 120,
            Weight = 150,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties = new Dictionary<string, object> {{"largeness", Size.Small}, {"physical deflection", 13}},
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            PreviousStageName = "sewer rat",
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Sqeek
        };
    }
}