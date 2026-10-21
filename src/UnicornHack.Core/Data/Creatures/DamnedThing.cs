namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature DamnedThing = new Creature
    {
        Name = "Damned Thing",
        SpeciesClass = SpeciesClass.Quadrupedal,
        Size = 8,
        Weight = 900,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        FocusBonus = -6,
        Armor = 3,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.Claws,
        LowerExtremities = ExtremityType.Claws,
        Quietness = 250,
        VisibilityLevel = 0,
        Infravision = true,
        InvisibilityDetection = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "unseen claw",
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
        EnglishDescription = "A low, four-legged shape whose outline is difficult to hold in view. When discernible, its heavy shoulders and long claws form a silhouette with no clear color or markings.",
        InitialLevel = 8,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable | CreatureGenerationFlags.BranchBoss,
        Behavior = AIBehavior.Stalking
    };
}
