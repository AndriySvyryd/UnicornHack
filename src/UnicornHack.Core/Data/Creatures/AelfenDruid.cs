namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature AelfenDruid = new Creature
    {
        Name = "aelfen druid",
        SpeciesClass = SpeciesClass.Humanoid,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "aelfen druid ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "aelfen druid ability #2",
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
                Name = "aelfen druid ability #3",
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
        InitialLevel = 8,
        GenerationWeight = "3",
        Behavior = AIBehavior.AlignmentAware | AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Speach,
        Sex = Sex.Male,
        Weight = 800,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        MagicResistance = 10,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true,
        InvisibilityDetection = true
    };
}
