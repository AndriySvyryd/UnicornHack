using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant WingedGargoyle = new CreatureVariant
        {
            Name = "winged gargoyle",
            Species = Species.Gargoyle,
            MovementDelay = 80,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Claw,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 100}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Claw,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 100}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "flight",
                "flight control",
                "humanoidness",
                "breathlessness",
                "oviparity",
                "stoning resistance"
            },
            ValuedProperties =
                new Dictionary<string, object> {{"thick hide", 3}, {"physical deflection", 24}, {"weight", 1200}},
            InitialLevel = 9,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            PreviousStageName = "gargoyle",
            Behavior = MonsterBehavior.MagicUser,
            Noise = ActorNoiseType.Grunt
        };
    }
}