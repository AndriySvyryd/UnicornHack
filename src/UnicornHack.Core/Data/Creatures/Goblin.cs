using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Goblin = new Creature
        {
            Name = "goblin",
            Species = Species.Goblin,
            MovementDelay = 200,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 20}}
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
            Behavior = AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Grunt,
            Size = 2,
            Weight = 400,
            Agility = 1,
            Constitution = 1,
            Intelligence = 1,
            Quickness = 1,
            Strength = 1,
            Willpower = 1,
            PhysicalDeflection = 10,
            TorsoType = TorsoType.Humanoid,
            UpperExtremeties = ExtremityType.GraspingFingers,
            LowerExtremeties = ExtremityType.Fingers,
            Infravisible = true,
            Infravision = true
        };
    }
}
