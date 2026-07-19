namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Eyedra = new Creature
    {
        Name = "eyedra",
        SpeciesClass = SpeciesClass.Aberration,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "eyedra ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "30*MightModifier()" } }
            },
            new Ability
            {
                Name = "eyedra ability #2",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Gaze,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 350,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Wither { Amount = "50*FocusModifier()" }, new Wither { Duration = EffectDuration.UntilTimeout, Amount = "50*FocusModifier()" } }
            },
            new Ability
            {
                Name = "eyedra ability #3",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Gaze,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 350,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Slow() }
            },
            new Ability
            {
                Name = "eyedra ability #4",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Gaze,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 350,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Sedate() }
            },
            new Ability
            {
                Name = "eyedra ability #5",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Gaze,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 350,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Confuse() }
            },
            new Ability
            {
                Name = "eyedra ability #6",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Gaze,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 350,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Daze() }
            },
            new Ability
            {
                Name = "eyedra ability #7",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Gaze,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 350,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new DrainEnergy { Amount = "3*FocusModifier()" } }
            }
        },
        InitialLevel = 8,
        GenerationWeight = "3",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.Wandering,
        Sex = Sex.None,
        Weight = 250,
        MovementDelay = 200,
        TurningDelay = 200,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        FocusBonus = -6,
        Armor = 3,
        MagicResistance = 17,
        ColdResistance = 75,
        HeadType = HeadType.None,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Flying,
        SlotCapacity = 0,
        Quietness = 250,
        Infravisible = true,
        Infravision = true
    };
}
