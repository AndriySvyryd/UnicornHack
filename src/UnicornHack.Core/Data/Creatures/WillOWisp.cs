namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature WillOWisp = new Creature
    {
        Name = "will-o-wisp",
        SpeciesClass = SpeciesClass.Extraplanar,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "will-o-wisp ability #1",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Explosion,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Confuse() }
            },
            new Ability
            {
                Name = "will-o-wisp ability #2",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 15,
                Action = AbilityAction.Gaze,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 300,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Fascinate() }
            }
        },
        InitialLevel = 3,
        GenerationWeight = "4",
        Sex = Sex.None,
        Size = 2,
        Weight = 0,
        MovementDelay = -20,
        TurningDelay = -20,
        Material = Material.Air,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -4,
        AcidResistance = 75,
        ColdResistance = 75,
        ElectricityResistance = 75,
        FireResistance = 75,
        BindResistance = 100,
        SuffocateResistance = 100,
        ToppleResistance = 100,
        VoidResistance = 75,
        HeadType = HeadType.None,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Flying,
        SlotCapacity = 0,
        EyeCount = 0,
        Quietness = 250,
        Infravisible = true,
        InvisibilityDetection = true,
        Mindless = true,
        NonAnimal = true
    };
}
