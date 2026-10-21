namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Crocodile = new Creature
    {
        Name = "crocodile",
        SpeciesClass = SpeciesClass.Reptile,
        MovementDelay = 33,
        TurningDelay = 33,
        Size = 8,
        Weight = 1500,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        FocusBonus = -6,
        Armor = 3,
        MagicResistance = 5,
        UpperExtremities = ExtremityType.None,
        RespirationType = RespirationType.Amphibious,
        LocomotionType = LocomotionType.Swimming,
        SlotCapacity = 1,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "crushing bite",
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
            },
            new Ability
            {
                Name = "rolling clamp",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Hug,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 300,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Bind() }
            }
        },
        EnglishDescription = "A low, armored reptile with a long, toothed snout and rows of raised scales along its back. Short legs support its thick body, which narrows into a muscular, flattened tail.",
        InitialLevel = 8,
        GenerationWeight = "0"
    };
}
