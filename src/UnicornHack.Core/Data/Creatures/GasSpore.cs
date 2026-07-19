namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature GasSpore = new Creature
    {
        Name = "gas spore",
        SpeciesClass = SpeciesClass.Plant,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "gas spore ability #1",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Explosion,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "140*FocusModifier()" }, new Stagger() }
            },
            new Ability
            {
                Name = "gas spore ability #2",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Explosion,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Deafen() }
            }
        },
        InitialLevel = 3,
        GenerationWeight = "2",
        Sex = Sex.None,
        Size = 2,
        Weight = 10,
        MovementDelay = 300,
        TurningDelay = 300,
        PerceptionBonus = -9,
        MightBonus = -9,
        SpeedBonus = -9,
        FocusBonus = -4,
        BlindResistance = 100,
        SuffocateResistance = 100,
        ToppleResistance = 50,
        HeadType = HeadType.None,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Flying,
        SlotCapacity = 0,
        EyeCount = 0,
        Quietness = 250,
        Mindless = true,
        NonAnimal = true
    };
}
