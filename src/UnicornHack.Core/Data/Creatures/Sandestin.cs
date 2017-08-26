using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Sandestin = new CreatureVariant
        {
            Name = "sandestin",
            Species = Species.Sandestin,
            SpeciesClass = SpeciesClass.ShapeChanger,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 7}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Poison {Damage = 4}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"infravision", "infravisibility", "humanoidness", "stoning resistance"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"physical deflection", 16},
                    {"magic resistance", 60},
                    {"weight", 1500}
                },
            InitialLevel = 13,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight(), Name = "hell"},
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Cuss
        };
    }
}