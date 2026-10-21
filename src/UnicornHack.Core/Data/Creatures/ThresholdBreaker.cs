namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature ThresholdBreaker = new Creature
    {
        Name = "Threshold-Breaker",
        SpeciesClass = SpeciesClass.Demon,
        Size = 16,
        Weight = 2250,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        FocusBonus = -6,
        Armor = 3,
        MagicResistance = 2,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "vault-cracking blow",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bludgeon,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "110*MightModifier()" } }
            }
        },
        EnglishDescription = "A huge, thick-necked humanoid with slab-like shoulders and a heavy, lowered brow. Its broad hands hang near its knees, and deep creases divide its dark, stone-rough skin.",
        InitialLevel = 9,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable | CreatureGenerationFlags.LevelBoss,
        Behavior = AIBehavior.NonRecruitable | AIBehavior.Stalking | AIBehavior.Displacing,
        Noise = ActorNoiseType.Boast
    };
}
