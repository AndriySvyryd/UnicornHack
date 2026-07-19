namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Lion = new Creature
    {
        Name = "lion",
        SpeciesClass = SpeciesClass.Feline,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "lion ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "lion ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "30*MightModifier()" } }
            },
            new Ability
            {
                Name = "lion ability #3",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "30*MightModifier()" } }
            }
        },
        InitialLevel = 6,
        Noise = ActorNoiseType.Growl,
        Size = 8,
        Weight = 600,
        MovementDelay = -20,
        TurningDelay = -20,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -6,
        Armor = 2,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true
    };
}
