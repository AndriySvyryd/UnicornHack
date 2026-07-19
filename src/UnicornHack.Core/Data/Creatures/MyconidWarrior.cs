namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature MyconidWarrior = new Creature
    {
        Name = "myconid warrior",
        SpeciesClass = SpeciesClass.Plant,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "myconid warrior ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ToxinDamage { Duration = EffectDuration.UntilTimeout, Amount = "30*MightModifier()" } }
            },
            new Ability
            {
                Name = "myconid warrior ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Stick() }
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
        DecayResistance = 50,
        SuffocateResistance = 100,
        HeadType = HeadType.None,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        SlotCapacity = 0,
        EyeCount = 0,
        Quietness = 250,
        Mindless = true,
        NonAnimal = true
    };
}
