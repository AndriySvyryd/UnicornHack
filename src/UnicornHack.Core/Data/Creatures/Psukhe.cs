namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Psukhe = new Creature
    {
        Name = "psukhe",
        SpeciesClass = SpeciesClass.Undead,
        Material = Material.Air,
        MovementDelay = 300,
        TurningDelay = 300,
        Weight = 0,
        PerceptionBonus = -4,
        MightBonus = -4,
        SpeedBonus = -4,
        HealingResistance = 200,
        Armor = 7,
        MagicResistance = 7,
        ColdResistance = 75,
        VoidResistance = 75,
        BindResistance = 100,
        StickResistance = 100,
        SuffocateResistance = 100,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Walking | LocomotionType.Phasing,
        SlotCapacity = 0,
        Infravision = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "withering touch",
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
                Name = "dread gaze",
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
        EnglishDescription = "A faint, upright human shape with soft contours and a face like a half-remembered reflection. Its translucent hands and shoulders trail away into a pale, drifting mist.",
        InitialLevel = 10,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.Wandering | AIBehavior.Stalking
    };
}
