using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature VampireBat = new Creature
        {
            Name = "vampire bat",
            Species = Species.Bat,
            SpeciesClass = SpeciesClass.Bird,
            Abilities =
                new HashSet<Ability>
                {
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
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "30*MightModifier()"}}
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
                        Effects = new List<Effect>
                        {
                            new ChangeProperty<int>
                            {
                                Duration = EffectDuration.UntilTimeout,
                                DurationAmount = "5",
                                PropertyName = "Might",
                                Value = -1
                            }
                        }
                    }
                },
            InitialLevel = 5,
            GenerationWeight = "5",
            PreviousStageName = "giant bat",
            Behavior = AIBehavior.Wandering,
            Noise = ActorNoiseType.Sqeek,
            Size = 1,
            Weight = 100,
            MovementDelay = -40,
            TurningDelay = -40,
            Perception = -7,
            Might = -8,
            Speed = -7,
            Focus = -8,
            Regeneration = 3,
            Armor = 2,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            LocomotionType = LocomotionType.Flying,
            InventorySize = 1,
            NoiseLevel = 0,
            Infravisible = true
        };
    }
}
