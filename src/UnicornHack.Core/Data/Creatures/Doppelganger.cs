using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Doppelganger = new CreatureVariant
        {
            Name = "doppelganger",
            Species = Species.Doppelganger,
            SpeciesClass = SpeciesClass.ShapeChanger,
            MovementDelay = 100,
            Weight = 1000,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 6}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"sleep resistance", "polymorph control", "infravisibility", "humanoidness"},
            ValuedProperties = new Dictionary<string, object> {{"physical deflection", 15}, {"magic resistance", 20}},
            InitialLevel = 9,
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Imitate
        };
    }
}