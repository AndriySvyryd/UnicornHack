namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Lemure = new Creature
    {
        Name = "lemure",
        SpeciesClass = SpeciesClass.Undead,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "lemure ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Paralyze() }
            },
            new Ability
            {
                Name = "lemure ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Slow() }
            }
        },
        InitialLevel = 12,
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
        Noise = ActorNoiseType.Howl,
        Weight = 0,
        MovementDelay = 20,
        TurningDelay = 20,
        Material = Material.Air,
        PerceptionBonus = -3,
        MightBonus = -4,
        SpeedBonus = -3,
        FocusBonus = 2,
        HealingResistance = 200,
        PhysicalResistance = 50,
        MagicResistance = 12,
        ColdResistance = 75,
        StickResistance = 100,
        SuffocateResistance = 100,
        VoidResistance = 75,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Walking | LocomotionType.Phasing,
        SlotCapacity = 0,
        Infravision = true,
        InvisibilityDetection = true
    };
}
