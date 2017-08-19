using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Barghest = new CreatureVariant
        {
            Name = "barghest",
            Species = Species.Dog,
            SpeciesClass = SpeciesClass.Canine | SpeciesClass.ShapeChanger,
            MovementDelay = 75,
            Weight = 1200,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 7}}
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
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties = new Dictionary<string, object> {{"physical deflection", 18}, {"magic resistance", 20}},
            InitialLevel = 9,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.Mountable,
            Noise = ActorNoiseType.Bark
        };
    }
}