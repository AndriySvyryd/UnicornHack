namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature QuiveringBlob = new Creature
    {
        Name = "quivering blob",
        SpeciesClass = SpeciesClass.Ooze,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "quivering blob ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "40*MightModifier()" } }
            }
        },
        InitialLevel = 5,
        GenerationWeight = "5",
        Behavior = AIBehavior.Wandering,
        Sex = Sex.None,
        Size = 2,
        Weight = 200,
        MovementDelay = 1100,
        TurningDelay = 1100,
        PerceptionBonus = -7,
        MightBonus = -8,
        SpeedBonus = -7,
        FocusBonus = -2,
        Armor = 1,
        BlindResistance = 100,
        CrippleResistance = 100,
        SuffocateResistance = 100,
        HeadType = HeadType.None,
        TorsoType = TorsoType.Amorphic,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        EyeCount = 0,
        Quietness = 250,
        Mindless = true,
        NonAnimal = true
    };
}
