using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature FireGiant = new Creature
        {
            Name = "fire giant",
            Species = Species.Giant,
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 110}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Punch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                }
            },
            InitialLevel = 9,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            GenerationFlags = GenerationFlags.SmallGroup,
            Behavior = AIBehavior.GemCollector | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Boast,
            Size = 16,
            Weight = 2250,
            Perception = 5,
            Might = 4,
            Speed = 5,
            Focus = 4,
            MagicDeflection = 2,
            PhysicalDeflection = 16,
            FireResistance = 75,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            Infravisible = true,
            Infravision = true
        };
    }
}
