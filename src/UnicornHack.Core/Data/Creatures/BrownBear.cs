namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature BrownBear = new Creature
    {
        Name = "brown bear",
        SpeciesClass = SpeciesClass.Quadrupedal,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "brown bear ability #1",
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
            }
        },
        InitialLevel = 10,
        GenerationWeight = "3",
        Noise = ActorNoiseType.Bark,
        Size = 8,
        Weight = 300,
        MovementDelay = -20,
        TurningDelay = -20,
        PerceptionBonus = -2,
        MightBonus = 3,
        SpeedBonus = -3,
        FocusBonus = -6,
        Armor = 1,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true
    };
}
