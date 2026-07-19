namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Cerberous = new Creature
    {
        Name = "Cerberous",
        SpeciesClass = SpeciesClass.Canine,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "Cerberous ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "100*MightModifier()" } }
            },
            new Ability
            {
                Name = "Cerberous ability #2",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Breath,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect>
                {
                    new FireDamage { Amount = "100*FocusModifier()" },
                    new FireDamage { Duration = EffectDuration.UntilTimeout, Amount = "100*FocusModifier()" }
                }
            }
        },
        InitialLevel = 12,
        GenerationWeight = "$branch == 'hell' ? 6 : 0",
        Noise = ActorNoiseType.Bark,
        Weight = 700,
        MovementDelay = -15,
        TurningDelay = -15,
        PerceptionBonus = -3,
        MightBonus = -4,
        SpeedBonus = -3,
        FocusBonus = -4,
        Armor = 4,
        MagicResistance = 10,
        FireResistance = 75,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true
    };
}
