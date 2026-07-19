namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature WingedGargoyle = new Creature
    {
        Name = "winged gargoyle",
        SpeciesClass = SpeciesClass.Construct,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "winged gargoyle ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "100*MightModifier()" } }
            },
            new Ability
            {
                Name = "winged gargoyle ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "100*MightModifier()" } }
            },
            new Ability
            {
                Name = "winged gargoyle ability #3",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "70*MightModifier()" } }
            }
        },
        InitialLevel = 9,
        GenerationWeight = "2",
        PreviousStageName = "gargoyle",
        Behavior = AIBehavior.MagicUser,
        Noise = ActorNoiseType.Grunt,
        Weight = 1200,
        MovementDelay = -20,
        TurningDelay = -20,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        FocusBonus = -6,
        Armor = 7,
        SedateResistance = 50,
        SuffocateResistance = 100,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Flying
    };
}
