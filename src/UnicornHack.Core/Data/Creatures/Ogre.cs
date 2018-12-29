using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Ogre = new Creature
        {
            Name = "ogre",
            Species = Species.Ogre,
            MovementDelay = 120,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 60}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Punch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                }
            },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            NextStageName = "ogre lord",
            GenerationFlags = GenerationFlags.SmallGroup,
            Behavior = AIBehavior.GoldCollector | AIBehavior.GemCollector | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Grunt,
            Size = 8,
            Weight = 1600,
            Perception = 3,
            Might = 2,
            Speed = 3,
            Focus = 2,
            PhysicalDeflection = 15,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            Infravisible = true
        };
    }
}
