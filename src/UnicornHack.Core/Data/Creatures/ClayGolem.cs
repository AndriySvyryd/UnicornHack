namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature ClayGolem = new Creature
    {
        Name = "clay golem",
        SpeciesClass = SpeciesClass.Construct,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "clay golem ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "160*MightModifier()" } }
            }
        },
        InitialLevel = 11,
        Sex = Sex.None,
        Size = 8,
        Weight = 1500,
        MovementDelay = 71,
        TurningDelay = 71,
        Material = Material.Mineral,
        PerceptionBonus = -4,
        MightBonus = -4,
        SpeedBonus = -4,
        Armor = 1,
        MagicResistance = 20,
        ElectricityResistance = 75,
        FascinateResistance = 100,
        SedateResistance = 100,
        SuffocateResistance = 100,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        Mindless = true,
        NonAnimal = true
    };
}
