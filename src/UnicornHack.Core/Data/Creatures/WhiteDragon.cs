namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature WhiteDragon = new Creature
    {
        Name = "white dragon",
        SpeciesClass = SpeciesClass.Reptile,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "white dragon ability #1",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Breath,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ColdDamage { Amount = "140*FocusModifier()" } }
            },
            new Ability
            {
                Name = "white dragon ability #2",
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
                Name = "white dragon ability #3",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "white dragon ability #4",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            }
        },
        InitialLevel = 15,
        GenerationWeight = "2",
        Behavior = AIBehavior.Mountable | AIBehavior.GoldCollector | AIBehavior.GemCollector,
        Noise = ActorNoiseType.Roar,
        Size = 32,
        Weight = 4500,
        MovementDelay = 33,
        TurningDelay = 33,
        PerceptionBonus = -2,
        MightBonus = -2,
        SpeedBonus = -2,
        FocusBonus = -2,
        Armor = 5,
        MagicResistance = 10,
        ColdResistance = 75,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        LocomotionType = LocomotionType.Flying,
        SlotCapacity = 1,
        Infravision = true,
        InvisibilityDetection = true
    };
}
