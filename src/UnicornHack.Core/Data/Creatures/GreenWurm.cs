using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature GreenWurm = new Creature
        {
            Name = "green wurm",
            Species = Species.Dragon,
            SpeciesClass = SpeciesClass.Reptile,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnRangedAttack,
                        Range = 20,
                        Action = AbilityAction.Breath,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new Blight {Damage = "140*FocusModifier()"}}
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnMeleeAttack,
                        Range = 1,
                        Action = AbilityAction.Bite,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "90*MightModifier()"}}
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnMeleeAttack,
                        Range = 1,
                        Action = AbilityAction.Claw,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "50*MightModifier()"}}
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnMeleeAttack,
                        Range = 1,
                        Action = AbilityAction.Claw,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "50*MightModifier()"}}
                    }
                },
            InitialLevel = 15,
            GenerationWeight = "2",
            Behavior = AIBehavior.Mountable | AIBehavior.GoldCollector | AIBehavior.GemCollector,
            Noise = ActorNoiseType.Roar,
            Size = 32,
            Weight = 4500,
            MovementDelay = 33,
            TurningDelay = 33,
            Perception = -2,
            Might = -2,
            Speed = -2,
            Focus = -2,
            Armor = 5,
            MagicResistance = 10,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            LocomotionType = LocomotionType.Walking,
            InventorySize = 1,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}
