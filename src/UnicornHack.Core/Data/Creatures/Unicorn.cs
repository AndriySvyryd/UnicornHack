namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Unicorn = new Creature
    {
        Name = "unicorn",
        SpeciesClass = SpeciesClass.Quadrupedal,
        MovementDelay = -50,
        TurningDelay = -50,
        Size = 8,
        Weight = 1300,
        PerceptionBonus = -7,
        MightBonus = -8,
        SpeedBonus = -7,
        FocusBonus = -8,
        Armor = 4,
        MagicResistance = 35,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "horn thrust",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Headbutt,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "60*MightModifier()" } }
            },
            new Ability
            {
                Name = "rear kick",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Kick,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "30*MightModifier()" } }
            }
        },
        EnglishDescription = "A slender, pale-coated equine with a flowing mane and a single spiraled horn rising from its forehead. Its fine ears stand upright, and its long tail fans into soft, silvery hair.",
        InitialLevel = 4,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.RangedPeaceful | AIBehavior.NonRecruitable | AIBehavior.Wandering,
        Noise = ActorNoiseType.Neigh
    };
}
