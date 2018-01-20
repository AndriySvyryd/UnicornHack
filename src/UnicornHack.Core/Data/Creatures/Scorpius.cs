using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Scorpius = new CreatureVariant
        {
            Name = "Scorpius",
            Species = Species.Scorpion,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 100,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Claw,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Sting,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Infect {Strength = 2}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Poison {Damage = 70}}
                }
            },
            SimpleProperties = new HashSet<string> {"animal body", "handlessness", "maleness", "stoning resistance"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 75},
                {"venom resistance", 75},
                {"physical deflection", 17},
                {"weight", 1000}
            },
            InitialLevel = 16,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.Covetous,
            Noise = ActorNoiseType.Quest
        };
    }
}