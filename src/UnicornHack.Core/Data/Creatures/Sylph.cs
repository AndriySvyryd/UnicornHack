namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Sylph = new Creature
    {
        Name = "sylph",
        SpeciesClass = SpeciesClass.Extraplanar,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "sylph ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Engulf { Amount = "400" } }
            },
            new Ability
            {
                Name = "sylph ability #2",
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Digestion,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Deafen() }
            }
        },
        InitialLevel = 8,
        GenerationWeight = "2",
        Sex = Sex.None,
        Size = 16,
        Weight = 0,
        MovementDelay = -67,
        TurningDelay = -67,
        Material = Material.Air,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        Armor = 4,
        MagicResistance = 15,
        BindResistance = 100,
        EngulfResistance = 100,
        SuffocateResistance = 100,
        HeadType = HeadType.None,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Flying,
        EyeCount = 0,
        VisibilityLevel = 0,
        Mindless = true,
        NonAnimal = true
    };
}
