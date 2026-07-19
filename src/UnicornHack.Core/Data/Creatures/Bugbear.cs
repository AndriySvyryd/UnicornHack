namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Bugbear = new Creature
    {
        Name = "bugbear",
        SpeciesClass = SpeciesClass.Humanoid,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "bugbear ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "bugbear ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage() }
            },
            new Ability
            {
                Name = "bugbear ability #3",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 8,
                Action = AbilityAction.Scream,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 300,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Taunt() }
            }
        },
        InitialLevel = 5,
        GenerationWeight = "4",
        Behavior = AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Growl,
        Size = 8,
        Weight = 1250,
        MovementDelay = 33,
        TurningDelay = 33,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        Armor = 2,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true
    };
}
