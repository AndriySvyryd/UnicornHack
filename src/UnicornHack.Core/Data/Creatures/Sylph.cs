namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Sylph = new Creature
    {
        Name = "sylph",
        SpeciesClass = SpeciesClass.Extraplanar,
        Material = Material.Air,
        MovementDelay = -67,
        TurningDelay = -67,
        Size = 16,
        Weight = 0,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        Armor = 4,
        MagicResistance = 15,
        BindResistance = 100,
        EngulfResistance = 100,
        SuffocateResistance = 100,
        HeadType = HeadType.None,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Flying,
        EyeCount = 0,
        VisibilityLevel = 0,
        Mindless = true,
        NonAnimal = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "enveloping gust",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Engulf { Amount = "400" } }
            },
            new Ability
            {
                Name = "deafening gale",
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Digestion,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Deafen() }
            }
        },
        EnglishDescription = "A twisting column of translucent air, outlined by fine spirals of dust and pale vapor. Its wispy edges stretch and fold without settling into a face or limbs.",
        InitialLevel = 8,
        GenerationWeight = "$branch == 'ruined stronghold' && $depth == 2 ? 2 : 0",
        Sex = Sex.None
    };
}
