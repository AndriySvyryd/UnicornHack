namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature AnimatedSkeleton = new Creature
    {
        Name = "animated skeleton",
        SpeciesClass = SpeciesClass.Undead,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "animated skeleton ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "30*MightModifier()" } }
            },
            new Ability
            {
                Name = "animated skeleton ability #2",
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
        InitialLevel = 3,
        GenerationWeight = "3",
        GenerationFlags = CreatureGenerationFlags.SmallGroup,
        Behavior = AIBehavior.WeaponCollector,
        Weight = 300,
        MovementDelay = 100,
        TurningDelay = 100,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -4,
        HealingResistance = 200,
        PhysicalResistance = 20,
        ColdResistance = 75,
        DoomResistance = 100,
        FrightenResistance = 100,
        SedateResistance = 100,
        SuffocateResistance = 100,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        Infravision = true,
        Mindless = true
    };
}
