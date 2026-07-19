namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Homunculus = new Creature
    {
        Name = "homunculus",
        SpeciesClass = SpeciesClass.Demon,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "homunculus ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage(), new Daze { Amount = "250*FocusModifier()" } }
            },
            new Ability
            {
                Name = "homunculus ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Sedate() }
            }
        },
        InitialLevel = 2,
        GenerationWeight = "4",
        Behavior = AIBehavior.Stalking,
        Sex = Sex.None,
        Size = 2,
        Weight = 60,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -4,
        Regeneration = 3,
        Armor = 2,
        MagicResistance = 5,
        Infravisible = true,
        Infravision = true,
        Mindless = true
    };
}
