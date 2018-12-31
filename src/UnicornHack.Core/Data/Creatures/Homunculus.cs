using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Homunculus = new Creature
        {
            Name = "homunculus",
            Species = Species.Homunculus,
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Sedate { Duration = EffectDuration.UntilTimeout, DurationAmount = 2}}
                }
            },
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Behavior = AIBehavior.Stalking,
            Sex = Sex.None,
            Size = 2,
            Weight = 60,
            Perception = 2,
            Might = 2,
            Speed = 2,
            Focus = 6,
            Regeneration = 3,
            MagicDeflection = 5,
            PhysicalDeflection = 14,
            Infravisible = true,
            Infravision = true,
            Mindless = true
        };
    }
}
