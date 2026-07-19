namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature MyconidSentry = new Creature
    {
        Name = "myconid sentry",
        SpeciesClass = SpeciesClass.Plant,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "myconid sentry ability #1",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Scream,
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
        Weight = 100,
        MovementDelay = 1100,
        TurningDelay = 1100,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -4,
        Armor = 1,
        BlindResistance = 100,
        FascinateResistance = 100,
        SuffocateResistance = 100,
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
