using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Halfling = new Creature
        {
            Name = "halfling",
            Species = Species.Halfling,
            MovementDelay = 133,
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
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                }
            },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.AlignmentAware | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Speach,
            Weight = 500,
            Agility = 4,
            Constitution = 1,
            Intelligence = 1,
            Quickness = 6,
            Strength = 1,
            Willpower = 1,
            MagicResistance = 10,
            PhysicalDeflection = 10,
            TorsoType = TorsoType.Humanoid,
            UpperExtremeties = ExtremityType.GraspingFingers,
            LowerExtremeties = ExtremityType.Fingers,
            Infravisible = true,
            Infravision = true
        };
    }
}
