using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Quasit = new Creature
        {
            Name = "quasit",
            Species = Species.Imp,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 80,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Claw,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 20}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Claw,
                    Cooldown = 100,
                    Effects = new HashSet<Effect>
                    {
                        new ChangeProperty<int> {PropertyName = "Speed", Value = -1, Duration = EffectDuration.UntilTimeout, DurationAmount = 5}
                    }
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 7F},
            Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
            Noise = ActorNoiseType.Cuss,
            Size = 2,
            Weight = 200,
            Perception = 2,
            Might = 2,
            Speed = 2,
            Focus = 2,
            Regeneration = 3,
            MagicDeflection = 10,
            PhysicalDeflection = 18,
            Infravisible = true,
            Infravision = true
        };
    }
}
