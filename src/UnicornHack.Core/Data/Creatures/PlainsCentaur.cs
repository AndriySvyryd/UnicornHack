using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature PlainsCentaur = new Creature
        {
            Name = "plains centaur",
            Species = Species.Centaur,
            MovementDelay = 66,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
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
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                }
            },
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            Behavior = AIBehavior.GoldCollector | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Speach,
            Size = 8,
            Weight = 2000,
            Perception = 3,
            Might = 2,
            Speed = 3,
            Focus = 2,
            PhysicalDeflection = 16,
            TorsoType = TorsoType.Humanoid,
            Infravisible = true
        };
    }
}
