namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Wolfwere = new Creature
    {
        Name = "wolfwere",
        SpeciesClass = SpeciesClass.Canine | SpeciesClass.ShapeChanger,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "wolfwere ability #1",
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
            },
            new Ability
            {
                Name = "wolfwere ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 250,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ConferLycanthropy { CreatureName = "wolfwere" } }
            }
        },
        InitialLevel = 5,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable,
        Noise = ActorNoiseType.Bark,
        Weight = 500,
        PerceptionBonus = -7,
        MightBonus = -8,
        SpeedBonus = -7,
        FocusBonus = -8,
        Regeneration = 3,
        Armor = 3,
        MagicResistance = 10,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true
    };
}
