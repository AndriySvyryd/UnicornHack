namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature WillOWisp = new Creature
    {
        Name = "will-o-wisp",
        SpeciesClass = SpeciesClass.Extraplanar,
        Material = Material.Air,
        MovementDelay = -20,
        TurningDelay = -20,
        Size = 2,
        Weight = 0,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -4,
        AcidResistance = 75,
        ColdResistance = 75,
        ElectricityResistance = 75,
        FireResistance = 75,
        VoidResistance = 75,
        BindResistance = 100,
        SuffocateResistance = 100,
        ToppleResistance = 100,
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
        NonAnimal = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "disorienting flare",
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
                Name = "entrancing glow",
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
        EnglishDescription = "A small globe of cold, pale light with a bright center and a ragged, flickering edge. Thin wisps trail from it as it drifts, with no solid body visible within the glow.",
        InitialLevel = 3,
        GenerationWeight = "$branch == 'ruined stronghold' && $depth <= 2 ? 2 : 0",
        Sex = Sex.None
    };
}
