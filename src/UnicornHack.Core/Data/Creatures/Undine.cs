namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Undine = new Creature
    {
        Name = "undine",
        SpeciesClass = SpeciesClass.Extraplanar,
        Material = Material.Water,
        MovementDelay = 100,
        TurningDelay = 100,
        Size = 16,
        Weight = 2500,
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
        NonAnimal = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "forceful strike",
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
                Name = "water-wreathed strike",
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
                Name = "undertow touch",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.OnMeleeHit,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                Accuracy = "10+PerceptionModifier()",
                Effects = new List<Effect> { new WaterDamage { Amount = "30*MightModifier()" } }
            }
        },
        EnglishDescription = "A clear, flowing mass of water that rises and curls in a continuous, upright wave. Light ripples across its rounded surface, and thin sheets spill from its sides before joining the body again.",
        InitialLevel = 8,
        GenerationWeight = "0",
        Sex = Sex.Female
    };
}
