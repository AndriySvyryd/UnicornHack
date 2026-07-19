namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Antlar = new Creature
    {
        Name = "antlar",
        SpeciesClass = SpeciesClass.MagicalBeast,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "antlar ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "70*MightModifier()" } }
            },
            new Ability
            {
                Name = "antlar ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "70*MightModifier()" } }
            },
            new Ability
            {
                Name = "antlar ability #3",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "60*MightModifier()" } }
            },
            new Ability
            {
                Name = "antlar ability #4",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Screech,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Confuse() }
            }
        },
        InitialLevel = 9,
        Size = 8,
        Weight = 1300,
        MovementDelay = 100,
        TurningDelay = 100,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        FocusBonus = -6,
        Armor = 4,
        MagicResistance = 12,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.Claws,
        LowerExtremities = ExtremityType.Claws,
        LocomotionType = LocomotionType.Walking | LocomotionType.Tunneling,
        Infravisible = true,
        Infravision = true
    };
}
