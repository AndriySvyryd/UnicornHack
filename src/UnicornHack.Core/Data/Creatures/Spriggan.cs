using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Spriggan = new Creature
    {
        Name = "spriggan",
        Species = Species.Nymph,
        SpeciesClass = SpeciesClass.Fey,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Sedate { Duration = EffectDuration.UntilTimeout, DurationAmount = "2" } }
            },
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Stun() }
            }
        },
        InitialLevel = 6,
        Behavior = AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Seduction,
        Sex = Sex.Female,
        Weight = 600,
        Perception = -8,
        Might = -7,
        Speed = -5,
        Focus = -5,
        MagicResistance = 10,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        LocomotionType = LocomotionType.Walking | LocomotionType.Teleportation,
        Infravisible = true
    };
}
