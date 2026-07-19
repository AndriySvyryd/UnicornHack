namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature FloatingEye = new Creature
    {
        Name = "floating eye",
        SpeciesClass = SpeciesClass.Aberration,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "floating eye ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.OnMeleeHit,
                Action = AbilityAction.Gaze,
                SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                Accuracy = "10+PerceptionModifier()",
                Effects = new List<Effect> { new Paralyze() }
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
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        MagicResistance = 5,
        HeadType = HeadType.None,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Flying,
        SlotCapacity = 0,
        Quietness = 250,
        Infravisible = true,
        Infravision = true,
        Mindless = true,
        NonAnimal = true
    };
}
