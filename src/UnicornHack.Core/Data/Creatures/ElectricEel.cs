namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature ElectricEel = new Creature
    {
        Name = "electric eel",
        SpeciesClass = SpeciesClass.Aquatic,
        MovementDelay = 20,
        TurningDelay = 20,
        Size = 8,
        Weight = 600,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -6,
        Armor = 6,
        ElectricityResistance = 75,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.Water,
        LocomotionType = LocomotionType.Swimming,
        SlotCapacity = 0,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "shocking bite",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ShockDamage { Amount = "140*MightModifier()" } }
            },
            new Ability
            {
                Name = "coiling bind",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Hug,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Bind() }
            }
        },
        EnglishDescription = "A long, smooth-bodied fish with a blunt head, a dark back and a paler underside. A continuous fin ripples along its belly as its muscular body bends through the water.",
        InitialLevel = 7,
        GenerationWeight = "0"
    };
}
