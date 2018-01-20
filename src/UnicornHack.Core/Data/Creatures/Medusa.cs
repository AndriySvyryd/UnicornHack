using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Medusa = new CreatureVariant
        {
            Name = "Medusa",
            Species = Species.Human,
            MovementDelay = 100,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Claw,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 40}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Envenom {Damage = 30}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Gaze,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Stone()}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Poison {Damage = 100}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "flight",
                "flight control",
                "amphibiousness",
                "infravisibility",
                "humanoidness",
                "femaleness",
                "stoning resistance"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 75},
                {"venom resistance", 75},
                {"physical deflection", 18},
                {"magic resistance", 50},
                {"weight", 1000}
            },
            InitialLevel = 20,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.RangedPeaceful,
            Noise = ActorNoiseType.Hiss
        };
    }
}