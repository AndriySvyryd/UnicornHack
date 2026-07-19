namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature ShockingSphere = new Creature
    {
        Name = "shocking sphere",
        SpeciesClass = SpeciesClass.Extraplanar,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "shocking sphere ability #1",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Explosion,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ShockDamage { Amount = "140*FocusModifier()" }, new Topple() }
            }
        },
        InitialLevel = 6,
        Sex = Sex.None,
        Size = 2,
        Weight = 10,
        MovementDelay = -8,
        TurningDelay = -8,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -6,
        Armor = 3,
        MagicResistance = 5,
        ElectricityResistance = 75,
        SuffocateResistance = 100,
        ToppleResistance = 100,
        HeadType = HeadType.None,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Flying,
        SlotCapacity = 0,
        Mindless = true,
        NonAnimal = true
    };
}
