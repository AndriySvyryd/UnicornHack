using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature FloatingEye = new Creature
    {
        Name = "floating eye",
        Species = Species.FloatingSphere,
        SpeciesClass = SpeciesClass.Aberration,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.OnMeleeHit,
                Action = AbilityAction.Gaze,
                SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                Accuracy = "10+PerceptionModifier()",
                Effects = new List<Effect> { new Paralyze { Duration = EffectDuration.UntilTimeout, DurationAmount = "200" } }
            }
        },
        InitialLevel = 2,
        GenerationWeight = "3",
        Behavior = AIBehavior.Wandering,
        Sex = Sex.None,
        Size = 2,
        Weight = 10,
        MovementDelay = 1100,
        TurningDelay = 1100,
        Perception = -8,
        Might = -8,
        Speed = -8,
        Focus = -8,
        MagicResistance = 5,
        HeadType = HeadType.None,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Flying,
        SlotCapacity = 0,
        NoiseLevel = 0,
        Infravisible = true,
        Infravision = true,
        Mindless = true,
        NonAnimal = true
    };
}
