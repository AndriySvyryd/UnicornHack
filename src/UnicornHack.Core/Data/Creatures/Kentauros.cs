namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Kentauros = new Creature
    {
        Name = "kentauros",
        SpeciesClass = SpeciesClass.Humanoid,
        MovementDelay = -40,
        TurningDelay = -40,
        Size = 8,
        Weight = 2000,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -6,
        Armor = 4,
        MagicResistance = 5,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Hooves,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "charging force",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
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
                Effects = new List<Effect> { new PhysicalDamage { Amount = "40*MightModifier()" } }
            }
        },
        EnglishDescription = "A broad human torso rises from the shoulders of a strong, horse-shaped body. A coarse mane trails down its neck, and four sturdy legs end in dark hooves beneath a short, glossy coat.",
        InitialLevel = 6,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Noise = ActorNoiseType.Speach
    };
}
