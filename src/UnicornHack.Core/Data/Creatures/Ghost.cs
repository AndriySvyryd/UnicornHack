namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Ghost = new Creature
    {
        Name = "ghost",
        SpeciesClass = SpeciesClass.Undead,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "ghost ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage(), new Wither() }
            },
            new Ability
            {
                Name = "ghost ability #2",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 10,
                Action = AbilityAction.Gaze,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 300,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Frighten() }
            }
        },
        InitialLevel = 10,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
        Weight = 0,
        MovementDelay = 300,
        TurningDelay = 300,
        Material = Material.Air,
        PerceptionBonus = -4,
        MightBonus = -4,
        SpeedBonus = -4,
        HealingResistance = 200,
        Armor = 7,
        MagicResistance = 7,
        ColdResistance = 75,
        BindResistance = 100,
        StickResistance = 100,
        SuffocateResistance = 100,
        VoidResistance = 75,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Walking | LocomotionType.Phasing,
        SlotCapacity = 0,
        Infravision = true
    };
}
