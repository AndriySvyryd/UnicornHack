using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Abollar = new Creature
        {
            Name = "abollar",
            Species = Species.Abollar,
            MovementDelay = 100,
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
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Suck,
                    Timeout = 1,
                    Effects = new HashSet<Effect>
                    {
                        new ChangeProperty<int>
                        {
                            PropertyName = "intelligence",
                            Value = -2,
                            Duration = 10
                        }
                    }
                }
            },
            InitialLevel = 9,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            NextStageName = "master abollar",
            Behavior = AIBehavior.GoldCollector | AIBehavior.GemCollector | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Gurgle,
            Size = 8,
            Weight = 1200,
            Agility = 5,
            Constitution = 5,
            Intelligence = 5,
            Quickness = 5,
            Strength = 5,
            Willpower = 5,
            MagicResistance = 80,
            PhysicalDeflection = 15,
            TorsoType = TorsoType.Humanoid,
            UpperExtremeties = ExtremityType.GraspingFingers,
            LowerExtremeties = ExtremityType.Fingers,
            LocomotionType = LocomotionType.Flying,
            Telepathic = 3,
            Infravisible = true,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}
