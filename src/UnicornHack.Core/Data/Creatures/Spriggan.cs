namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Spriggan = new Creature
    {
        Name = "spriggan",
        SpeciesClass = SpeciesClass.Fey,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "spriggan ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Sedate() }
            },
            new Ability
            {
                Name = "spriggan ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Stun() }
            }
        },
        InitialLevel = 6,
        Behavior = AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Seduction,
        Sex = Sex.Female,
        Weight = 600,
        PerceptionBonus = -8,
        MightBonus = -7,
        SpeedBonus = -5,
        FocusBonus = -5,
        MagicResistance = 10,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        LocomotionType = LocomotionType.Walking | LocomotionType.Teleportation,
        Infravisible = true
    };
}
