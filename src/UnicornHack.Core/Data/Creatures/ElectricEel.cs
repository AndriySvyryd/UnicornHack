namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature ElectricEel = new Creature
    {
        Name = "electric eel",
        SpeciesClass = SpeciesClass.Aquatic,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "electric eel ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ShockDamage { Amount = "140*MightModifier()" } }
            },
            new Ability
            {
                Name = "electric eel ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Hug,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Bind() }
            }
        },
        InitialLevel = 7,
        PreviousStageName = "giant eel",
        Size = 8,
        Weight = 600,
        MovementDelay = 20,
        TurningDelay = 20,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -6,
        Armor = 6,
        ElectricityResistance = 75,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.Water,
        LocomotionType = LocomotionType.Swimming,
        SlotCapacity = 0
    };
}
