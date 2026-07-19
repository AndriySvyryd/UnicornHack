namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Crocodile = new Creature
    {
        Name = "crocodile",
        SpeciesClass = SpeciesClass.Reptile,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "crocodile ability #1",
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
            }
        },
        InitialLevel = 8,
        GenerationWeight = "2",
        PreviousStageName = "baby crocodile",
        Size = 8,
        Weight = 1500,
        MovementDelay = 33,
        TurningDelay = 33,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        FocusBonus = -6,
        Armor = 3,
        MagicResistance = 5,
        UpperExtremities = ExtremityType.None,
        RespirationType = RespirationType.Amphibious,
        LocomotionType = LocomotionType.Swimming,
        SlotCapacity = 1
    };
}
