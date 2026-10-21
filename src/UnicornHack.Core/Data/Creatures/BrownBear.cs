namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature BrownBear = new Creature
    {
        Name = "brown bear",
        SpeciesClass = SpeciesClass.Quadrupedal,
        MovementDelay = -20,
        TurningDelay = -20,
        Size = 8,
        Weight = 300,
        PerceptionBonus = -2,
        MightBonus = 3,
        SpeedBonus = -3,
        FocusBonus = -6,
        Armor = 1,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "mauling bite",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            },
            new Ability
            {
                Name = "rearing swat",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 300,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "30*MightModifier()" }, new Topple() }
            }
        },
        EnglishDescription = "A bulky bear with dense brown fur, a pronounced shoulder hump and a broad, pale muzzle. Long curved claws extend from its heavy forepaws.",
        InitialLevel = 10,
        GenerationWeight = "0",
        Behavior = AIBehavior.Wandering,
        Noise = ActorNoiseType.Bark
    };
}
