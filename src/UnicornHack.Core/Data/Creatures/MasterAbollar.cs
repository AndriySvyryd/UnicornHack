using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature MasterAbollar = new Creature
        {
            Name = "master abollar",
            Species = Species.Abollar,
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 40}}
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
            InitialLevel = 13,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            PreviousStageName = "abollar",
            Behavior = AIBehavior.GoldCollector | AIBehavior.GemCollector | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Gurgle,
            Size = 8,
            Weight = 1200,
            Agility = 7,
            Constitution = 7,
            Intelligence = 7,
            Quickness = 7,
            Strength = 7,
            Willpower = 7,
            MagicResistance = 90,
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
