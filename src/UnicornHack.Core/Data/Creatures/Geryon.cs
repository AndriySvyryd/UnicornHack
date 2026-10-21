namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Geryon = new Creature
    {
        Name = "Geryon",
        SpeciesClass = SpeciesClass.Aberration,
        MovementDelay = 300,
        TurningDelay = 300,
        Size = 16,
        Weight = 2500,
        PerceptionBonus = 6,
        MightBonus = 6,
        SpeedBonus = 6,
        FocusBonus = 6,
        Armor = 6,
        MagicResistance = 37,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true,
        InvisibilityDetection = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "first aspect strike",
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
                Name = "second aspect strike",
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
            }
        },
        EnglishDescription = "An immense humanoid with an unusually broad chest and thick, knotted limbs. Deep ridges of muscle give its torso a crowded, uneven outline beneath coarse, dark skin.",
        InitialLevel = 30,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable | CreatureGenerationFlags.BranchBoss,
        Behavior = AIBehavior.Stalking | AIBehavior.Roaming,
        Noise = ActorNoiseType.Cuss,
        Sex = Sex.Male
    };
}
