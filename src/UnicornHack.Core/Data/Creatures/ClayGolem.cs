namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature ClayGolem = new Creature
    {
        Name = "clay golem",
        SpeciesClass = SpeciesClass.Construct,
        Material = Material.Mineral,
        MovementDelay = 71,
        TurningDelay = 71,
        Size = 8,
        Weight = 1500,
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
        NonAnimal = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "earthen fist",
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
        EnglishDescription = "A heavy human-shaped figure molded from dull, earthen clay. Thumb marks and shallow cracks cover its surface; the face is little more than a few pressed hollows.",
        InitialLevel = 11,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.NonRecruitable,
        Sex = Sex.None
    };
}
