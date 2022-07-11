namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature ElectricEel = new Creature
    {
        Name = "electric eel",
        Species = Species.Eel,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Shock { Damage = "140*MightModifier()" } }
            },
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Hug,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Bind { Duration = EffectDuration.UntilTimeout, DurationAmount = "10" } }
            }
        },
        InitialLevel = 7,
        PreviousStageName = "giant eel",
        Size = 8,
        Weight = 600,
        MovementDelay = 20,
        TurningDelay = 20,
        Perception = -6,
        Might = -6,
        Speed = -6,
        Focus = -6,
        Armor = 6,
        ElectricityResistance = 75,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.Water,
        LocomotionType = LocomotionType.Swimming,
        SlotCapacity = 0
    };
}
