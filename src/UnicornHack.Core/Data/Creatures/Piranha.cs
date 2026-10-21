namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Piranha = new Creature
    {
        Name = "piranha",
        SpeciesClass = SpeciesClass.Aquatic,
        Size = 1,
        Weight = 60,
        PerceptionBonus = -7,
        MightBonus = -8,
        SpeedBonus = -7,
        FocusBonus = -8,
        Armor = 3,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.Water,
        LocomotionType = LocomotionType.Swimming,
        SlotCapacity = 0,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "tearing bite",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "70*MightModifier()" } }
            },
            new Ability
            {
                Name = "schooling rush",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 300,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" }, new Stagger() }
            }
        },
        EnglishDescription = "A deep-bodied fish with bright silver scales, a dark back and a reddish belly. Its blunt head carries a heavy lower jaw lined with close-set triangular teeth.",
        InitialLevel = 5,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.SmallGroup
    };
}
