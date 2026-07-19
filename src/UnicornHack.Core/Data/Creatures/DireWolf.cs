namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature DireWolf = new Creature
    {
        Name = "dire wolf",
        SpeciesClass = SpeciesClass.Canine,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "dire wolf ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect>
                {
                    new PhysicalDamage { Amount = "70*MightModifier()" },
                    new BleedDamage { Duration = EffectDuration.UntilTimeout, Amount = "20*MightModifier()" }
                }
            }
        },
        InitialLevel = 7,
        GenerationWeight = "3",
        GenerationFlags = CreatureGenerationFlags.SmallGroup,
        Noise = ActorNoiseType.Bark,
        Weight = 1200,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -6,
        Armor = 3,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true
    };
}
