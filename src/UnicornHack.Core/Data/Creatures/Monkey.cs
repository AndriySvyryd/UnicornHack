namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Monkey = new Creature
    {
        Name = "monkey",
        SpeciesClass = SpeciesClass.Quadrupedal,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "monkey ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            },
            new Ability
            {
                Name = "monkey ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "25*MightModifier()" } }
            }
        },
        InitialLevel = 2,
        GenerationWeight = "2",
        Noise = ActorNoiseType.Growl,
        Size = 2,
        Weight = 100,
        MovementDelay = -34,
        TurningDelay = -34,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        Armor = 2,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true
    };
}
