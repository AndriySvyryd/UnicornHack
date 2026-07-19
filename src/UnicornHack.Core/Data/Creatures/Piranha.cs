namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Piranha = new Creature
    {
        Name = "piranha",
        SpeciesClass = SpeciesClass.Aquatic,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "piranha ability #1",
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
            }
        },
        InitialLevel = 5,
        GenerationFlags = CreatureGenerationFlags.SmallGroup,
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
        SlotCapacity = 0
    };
}
