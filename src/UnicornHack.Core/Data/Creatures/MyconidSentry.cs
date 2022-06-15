using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature MyconidSentry = new Creature
    {
        Name = "myconid sentry",
        Species = Species.Fungus,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Scream,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Deafen { Duration = EffectDuration.UntilTimeout, DurationAmount = "3" } }
            }
        },
        InitialLevel = 3,
        GenerationWeight = "2",
        Sex = Sex.None,
        Size = 2,
        Weight = 100,
        MovementDelay = 1100,
        TurningDelay = 1100,
        Perception = -8,
        Might = -8,
        Speed = -8,
        Focus = -4,
        Armor = 1,
        HeadType = HeadType.None,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        SlotCapacity = 0,
        EyeCount = 0,
        Mindless = true,
        NonAnimal = true
    };
}
