namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Cerberus = new Creature
    {
        Name = "Cerberus",
        SpeciesClass = SpeciesClass.Canine,
        MovementDelay = -15,
        TurningDelay = -15,
        Weight = 700,
        PerceptionBonus = -3,
        MightBonus = -4,
        SpeedBonus = -3,
        FocusBonus = -4,
        Armor = 4,
        MagicResistance = 10,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "threefold bite",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "100*MightModifier()" } }
            }
        },
        EnglishDescription = "A massive, three-headed hound with a deep chest and a coarse black coat. Three thick necks crowd above its shoulders, each ending in a broad muzzle and a pair of watchful eyes.",
        InitialLevel = 12,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable | CreatureGenerationFlags.BranchBoss,
        Behavior = AIBehavior.NonRecruitable | AIBehavior.Roaming,
        Noise = ActorNoiseType.Bark
    };
}
