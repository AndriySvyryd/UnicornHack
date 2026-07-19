namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature DwarvenWarrior = new Creature
    {
        Name = "dwarven warrior",
        SpeciesClass = SpeciesClass.Humanoid,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "dwarven warrior ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "30*MightModifier()" } }
            },
            new Ability
            {
                Name = "dwarven warrior ability #2",
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
        MovementDelay = 100,
        TurningDelay = 100,
        Weight = 900,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        MagicResistance = 5,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        LocomotionType = LocomotionType.Walking | LocomotionType.ToolTunneling,
        Infravisible = true,
        Infravision = true,
        InitialLevel = 2,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.AlignmentAware | AIBehavior.GoldCollector | AIBehavior.GemCollector | AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Speach
    };
}
