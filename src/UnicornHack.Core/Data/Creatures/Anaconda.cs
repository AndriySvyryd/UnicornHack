using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Anaconda = new Creature
    {
        Name = "anaconda",
        Species = Species.Snake,
        SpeciesClass = SpeciesClass.Reptile,
        Abilities = new HashSet<Ability>
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
                Effects = new List<Effect> { new PhysicalDamage { Damage = "20*MightModifier()" } }
            },
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Hug,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Damage = "50*MightModifier()" } }
            },
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Hug,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Bind { Duration = EffectDuration.UntilTimeout, DurationAmount = "2" } }
            }
        },
        InitialLevel = 6,
        GenerationWeight = "2",
        Noise = ActorNoiseType.Hiss,
        Weight = 250,
        MovementDelay = 300,
        TurningDelay = 300,
        Perception = -6,
        Might = -6,
        Speed = -6,
        Focus = -6,
        Armor = 2,
        TorsoType = TorsoType.Serpentine,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        LocomotionType = LocomotionType.Swimming,
        SlotCapacity = 0,
        Infravision = true
    };
}
