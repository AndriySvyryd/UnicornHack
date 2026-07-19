namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Naga = new Creature
    {
        Name = "naga",
        SpeciesClass = SpeciesClass.Aberration,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "naga ability #1",
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
                Name = "naga ability #2",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Spit,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Corrode() }
            }
        },
        InitialLevel = 8,
        NextStageName = "greater naga",
        Noise = ActorNoiseType.Hiss,
        Size = 16,
        Weight = 1500,
        MovementDelay = -15,
        TurningDelay = -15,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        FocusBonus = -6,
        Armor = 4,
        MagicResistance = 5,
        AcidResistance = 75,
        TorsoType = TorsoType.Serpentine,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        SlotCapacity = 1,
        Infravision = true
    };
}
