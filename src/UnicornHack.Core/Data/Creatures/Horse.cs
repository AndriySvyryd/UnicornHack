using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Horse = new CreatureVariant
        {
            Name = "horse",
            Species = Species.Horse,
            SpeciesClass = SpeciesClass.Quadrupedal,
            MovementDelay = 60,
            Weight = 1500,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Kick,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 4}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties = new Dictionary<string, object> {{"largeness", Size.Large}, {"physical deflection", 15}},
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 7F},
            PreviousStageName = "pony",
            NextStageName = "warhorse",
            Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Mountable | MonsterBehavior.Wandering,
            Noise = ActorNoiseType.Neigh
        };
    }
}