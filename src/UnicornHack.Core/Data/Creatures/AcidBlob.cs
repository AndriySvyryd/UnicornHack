namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature AcidBlob = new Creature
    {
        Name = "acid blob",
        SpeciesClass = SpeciesClass.Ooze,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "splashback",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.OnMeleeHit,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                Accuracy = "10+PerceptionModifier()",
                Effects = new List<Effect> { new Corrode() }
            }
        },
        InitialLevel = 5,
        GenerationWeight = "2",
        Behavior = AIBehavior.Wandering,
        Sex = Sex.None,
        Size = 1,
        Weight = 30,
        MovementDelay = 300,
        TurningDelay = 50,
        PerceptionBonus = -9,
        MightBonus = -5,
        SpeedBonus = -9,
        FocusBonus = -4,
        Armor = 1,
        AcidResistance = 75,
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
