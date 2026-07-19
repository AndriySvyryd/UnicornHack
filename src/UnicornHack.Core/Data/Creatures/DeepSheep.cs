namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature DeepSheep = new Creature
    {
        Name = "deep sheep",
        SpeciesClass = SpeciesClass.Quadrupedal,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "deep sheep ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Headbutt,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "40*MightModifier()" } }
            },
            new Ability
            {
                Name = "deep sheep ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            }
        },
        InitialLevel = 2,
        GenerationWeight = "4",
        GenerationFlags = CreatureGenerationFlags.SmallGroup,
        Noise = ActorNoiseType.Bleat,
        Weight = 600,
        MovementDelay = 33,
        TurningDelay = 33,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        Armor = 1,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        AcuityLevel = 0,
        Infravisible = true
    };
}
