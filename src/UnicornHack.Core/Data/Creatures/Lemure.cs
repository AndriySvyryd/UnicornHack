namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Lemure = new Creature
    {
        Name = "lemure",
        SpeciesClass = SpeciesClass.Undead,
        Material = Material.Air,
        MovementDelay = 20,
        TurningDelay = 20,
        Weight = 0,
        PerceptionBonus = -3,
        MightBonus = -4,
        SpeedBonus = -3,
        FocusBonus = 2,
        HealingResistance = 200,
        PhysicalResistance = 50,
        MagicResistance = 12,
        ColdResistance = 75,
        VoidResistance = 75,
        StickResistance = 100,
        SuffocateResistance = 100,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Walking | LocomotionType.Phasing,
        SlotCapacity = 0,
        Infravision = true,
        InvisibilityDetection = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "numbing touch",
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
                Name = "delaying touch",
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
        EnglishDescription = "A dim, slouching human outline with a blurred face and hanging arms. Its translucent edges fray into thin wisps, leaving the surroundings faintly visible through its body.",
        InitialLevel = 12,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
        Noise = ActorNoiseType.Howl
    };
}
