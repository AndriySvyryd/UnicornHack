namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Undine = new Creature
    {
        Name = "undine",
        SpeciesClass = SpeciesClass.Extraplanar,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "undine ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "170*MightModifier()" } }
            },
            new Ability
            {
                Name = "undine ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new WaterDamage { Amount = "30*MightModifier()" } }
            },
            new Ability
            {
                Name = "undine ability #3",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.OnMeleeHit,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                Accuracy = "10+PerceptionModifier()",
                Effects = new List<Effect> { new WaterDamage { Amount = "30*MightModifier()" } }
            }
        },
        InitialLevel = 8,
        GenerationWeight = "2",
        Sex = Sex.Female,
        Size = 16,
        Weight = 2500,
        MovementDelay = 100,
        TurningDelay = 100,
        Material = Material.Water,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        BonusHitPoints = 70,
        Armor = 4,
        MagicResistance = 15,
        WaterResistance = 200,
        HeadType = HeadType.None,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Swimming,
        EyeCount = 0,
        Mindless = true,
        NonAnimal = true
    };
}
