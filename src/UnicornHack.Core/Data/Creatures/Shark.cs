namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Shark = new Creature
    {
        Name = "shark",
        SpeciesClass = SpeciesClass.Aquatic,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "shark ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect>
                {
                    new PhysicalDamage { Amount = "170*MightModifier()" },
                    new BleedDamage { Duration = EffectDuration.UntilTimeout, Amount = "40*MightModifier()" }
                }
            }
        },
        InitialLevel = 7,
        Size = 8,
        Weight = 1000,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -6,
        Armor = 4,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.Water,
        LocomotionType = LocomotionType.Swimming,
        SlotCapacity = 0
    };
}
