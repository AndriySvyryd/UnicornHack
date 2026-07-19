namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Hyenawere = new Creature
    {
        Name = "hyenawere",
        SpeciesClass = SpeciesClass.Canine | SpeciesClass.ShapeChanger,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "hyenawere ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "hyenawere ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 250,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ConferLycanthropy { CreatureName = "hyenawere" } }
            }
        },
        InitialLevel = 3,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable,
        Noise = ActorNoiseType.Laugh,
        Size = 2,
        Weight = 150,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        Regeneration = 3,
        Armor = 2,
        MagicResistance = 5,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true
    };
}
