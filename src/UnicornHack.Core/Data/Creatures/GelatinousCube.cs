namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature GelatinousCube = new Creature
    {
        Name = "gelatinous cube",
        SpeciesClass = SpeciesClass.Ooze,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "gelatinous cube ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Paralyze() }
            },
            new Ability
            {
                Name = "gelatinous cube ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 350,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Engulf { Amount = "400" } }
            },
            new Ability
            {
                Name = "gelatinous cube ability #3",
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Digestion,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ToxinDamage { Duration = EffectDuration.UntilTimeout, Amount = "10*MightModifier()" }, new Corrode() }
            },
            new Ability
            {
                Name = "gelatinous cube ability #4",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.OnMeleeHit,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                Accuracy = "10+PerceptionModifier()",
                Effects = new List<Effect> { new Paralyze() }
            }
        },
        InitialLevel = 6,
        GenerationWeight = "4",
        Behavior = AIBehavior.Wandering | AIBehavior.WeaponCollector,
        Sex = Sex.None,
        Size = 8,
        Weight = 600,
        MovementDelay = 100,
        TurningDelay = 100,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -2,
        Armor = 1,
        AcidResistance = 75,
        ColdResistance = 75,
        ElectricityResistance = 75,
        FireResistance = 75,
        BlindResistance = 100,
        ParalyzeResistance = 100,
        SuffocateResistance = 100,
        HeadType = HeadType.None,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        EyeCount = 0,
        Quietness = 250,
        Mindless = true,
        NonAnimal = true
    };
}
