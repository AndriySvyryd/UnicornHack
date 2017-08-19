using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant MountainNymph = new CreatureVariant
        {
            Name = "mountain nymph",
            Species = Species.Nymph,
            SpeciesClass = SpeciesClass.Fey,
            MovementDelay = 100,
            Weight = 600,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Seduce()}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new StealItem()}
                    }
                },
            SimpleProperties = new HashSet<string> {"teleportation", "humanoidness", "infravisibility", "femaleness"},
            ValuedProperties = new Dictionary<string, object> {{"physical deflection", 11}, {"magic resistance", 20}},
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Behavior = MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Seduction
        };
    }
}