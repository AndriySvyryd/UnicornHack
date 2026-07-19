namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature GrayOoze = new Creature
    {
        Name = "gray ooze",
        SpeciesClass = SpeciesClass.Ooze,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "gray ooze ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "90*MightModifier()" } }
            },
            new Ability
            {
                Name = "gray ooze ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new WaterDamage { Amount = "90*MightModifier()" } }
            },
            new Ability
            {
                Name = "gray ooze ability #3",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.OnMeleeHit,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                Accuracy = "10+PerceptionModifier()",
                Effects = new List<Effect> { new WaterDamage { Amount = "40*MightModifier()" } }
            }
        },
        InitialLevel = 3,
        GenerationWeight = "2",
        Sex = Sex.None,
        Weight = 500,
        MovementDelay = 1100,
        TurningDelay = 1100,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -4,
        Armor = 1,
        AcidResistance = 75,
        ColdResistance = 75,
        FireResistance = 75,
        BlindResistance = 100,
        CrippleResistance = 100,
        SuffocateResistance = 100,
        HeadType = HeadType.None,
        TorsoType = TorsoType.Amorphic,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        EyeCount = 0,
        Quietness = 250,
        Mindless = true,
        NonAnimal = true
    };
}
