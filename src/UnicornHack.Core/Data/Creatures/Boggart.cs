namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Boggart = new Creature
    {
        Name = "boggart",
        SpeciesClass = SpeciesClass.Humanoid,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "boggart ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            },
            new Ability
            {
                Name = "boggart ability #2",
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
            }
        },
        InitialLevel = 1,
        Behavior = AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Grunt,
        Size = 2,
        Weight = 400,
        MovementDelay = 100,
        TurningDelay = 100,
        PerceptionBonus = -9,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -9,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true
    };
}
