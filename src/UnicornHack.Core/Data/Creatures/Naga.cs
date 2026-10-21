namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Naga = new Creature
    {
        Name = "naga",
        SpeciesClass = SpeciesClass.Reptile,
        MovementDelay = -15,
        TurningDelay = -15,
        Size = 16,
        Weight = 1500,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        FocusBonus = -6,
        Armor = 4,
        MagicResistance = 5,
        TorsoType = TorsoType.Serpentine,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        SlotCapacity = 1,
        Infravision = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "defensive bite",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "70*MightModifier()" } }
            }
        },
        EnglishDescription = "A large, upright serpent with a long, tapering body and close-set, lustrous scales. Its broad head rises from a graceful neck, and pale plates run along the underside of its coils.",
        InitialLevel = 8,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.NonRecruitable | AIBehavior.Wandering,
        Noise = ActorNoiseType.Hiss
    };
}
