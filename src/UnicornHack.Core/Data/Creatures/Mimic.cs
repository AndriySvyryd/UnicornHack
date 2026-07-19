namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Mimic = new Creature
    {
        Name = "mimic",
        SpeciesClass = SpeciesClass.ShapeChanger,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "mimic ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "90*MightModifier()" } }
            },
            new Ability
            {
                Name = "mimic ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Stick() }
            }
        },
        InitialLevel = 8,
        Weight = 600,
        MovementDelay = 300,
        TurningDelay = 300,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        FocusBonus = -6,
        Armor = 1,
        MagicResistance = 5,
        AcidResistance = 75,
        CrippleResistance = 50,
        SuffocateResistance = 100,
        HeadType = HeadType.None,
        TorsoType = TorsoType.Amorphic,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        EyeCount = 0,
        Quietness = 250,
        VisibilityLevel = 1,
        Infravisible = true,
        Clingy = true
    };
}
