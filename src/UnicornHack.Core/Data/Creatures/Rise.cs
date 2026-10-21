namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Rise = new Creature
    {
        Name = "Rise",
        SpeciesClass = SpeciesClass.Humanoid,
        Size = 16,
        Weight = 2250,
        PerceptionBonus = -4,
        MightBonus = -4,
        SpeedBonus = -4,
        FocusBonus = -4,
        Armor = 3,
        MagicResistance = 5,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "immense strength",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "110*MightModifier()" } }
            },
            new Ability
            {
                Name = "crushing fist",
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
        EnglishDescription = "A massive humanoid with a broad, weathered face and a thick, tangled beard. Its limbs are heavy and blunt, with rough clothing stretched across a barrel-like chest.",
        InitialLevel = 10,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable | CreatureGenerationFlags.BranchBoss,
        Noise = ActorNoiseType.Boast
    };
}
