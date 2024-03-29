namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature MyconidWarrior = new Creature
    {
        Name = "myconid warrior",
        Species = Species.Fungus,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Blight { Damage = "30*MightModifier()" } }
            },
            new Ability
            {
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
        InitialLevel = 3,
        GenerationWeight = "2",
        Sex = Sex.None,
        Size = 2,
        Weight = 100,
        MovementDelay = 1100,
        TurningDelay = 1100,
        Perception = -8,
        Might = -8,
        Speed = -8,
        Focus = -4,
        Armor = 1,
        HeadType = HeadType.None,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        SlotCapacity = 0,
        EyeCount = 0,
        NoiseLevel = 0,
        Mindless = true,
        NonAnimal = true
    };
}
