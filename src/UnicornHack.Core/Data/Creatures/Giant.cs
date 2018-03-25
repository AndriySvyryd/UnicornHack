using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Giant = new Creature
        {
            Name = "giant",
            Species = Species.Giant,
            MovementDelay = 200,
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
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                }
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonGenocidable,
            Behavior = AIBehavior.GemCollector | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Boast,
            Size = 16,
            Weight = 2250,
            Agility = 4,
            Constitution = 4,
            Intelligence = 4,
            Quickness = 4,
            Strength = 4,
            Willpower = 4,
            PhysicalDeflection = 14,
            TorsoType = TorsoType.Humanoid,
            UpperExtremeties = ExtremityType.GraspingFingers,
            LowerExtremeties = ExtremityType.Fingers,
            Infravisible = true,
            Infravision = true
        };
    }
}
