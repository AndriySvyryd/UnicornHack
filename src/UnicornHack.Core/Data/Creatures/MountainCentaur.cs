using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature MountainCentaur = new Creature
        {
            Name = "mountain centaur",
            Species = Species.Centaur,
            MovementDelay = 60,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Punch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Kick,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 40}}
                }
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            Behavior = AIBehavior.GoldCollector | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Speach,
            Size = 8,
            Weight = 2000,
            Perception = 4,
            Might = 4,
            Speed = 4,
            Focus = 4,
            MagicDeflection = 5,
            PhysicalDeflection = 18,
            TorsoType = TorsoType.Humanoid,
            Infravisible = true
        };
    }
}
