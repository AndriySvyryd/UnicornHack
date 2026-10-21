namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Spartos = new Creature
    {
        Name = "spartos",
        SpeciesClass = SpeciesClass.Humanoid,
        MovementDelay = 100,
        TurningDelay = 100,
        Weight = 300,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -4,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Mindless = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "martial force",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "30*MightModifier()" } }
            },
            new Ability
            {
                Name = "close strike",
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
        EnglishDescription = "A solidly built humanoid with earth-colored skin and close-cropped, wiry hair. Dust cakes its broad hands and settles in the folds of its stiff, rough-spun clothing.",
        InitialLevel = 3,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.SmallGroup
    };
}
