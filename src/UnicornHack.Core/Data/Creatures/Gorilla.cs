namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Gorilla = new Creature
    {
        Name = "gorilla",
        SpeciesClass = SpeciesClass.Quadrupedal,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "gorilla ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            },
            new Ability
            {
                Name = "gorilla ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            },
            new Ability
            {
                Name = "gorilla ability #3",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "40*MightModifier()" } }
            }
        },
        InitialLevel = 6,
        Noise = ActorNoiseType.Growl,
        Weight = 1250,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -6,
        Armor = 2,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.Fingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true
    };
}
