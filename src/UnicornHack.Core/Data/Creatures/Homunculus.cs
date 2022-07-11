namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Homunculus = new Creature
    {
        Name = "homunculus",
        Species = Species.Homunculus,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Damage = "10*MightModifier()" } }
            },
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Sedate { Duration = EffectDuration.UntilTimeout, DurationAmount = "2" } }
            }
        },
        InitialLevel = 2,
        GenerationWeight = "4",
        Behavior = AIBehavior.Stalking,
        Sex = Sex.None,
        Size = 2,
        Weight = 60,
        Perception = -8,
        Might = -8,
        Speed = -8,
        Focus = -4,
        Regeneration = 3,
        Armor = 2,
        MagicResistance = 5,
        Infravisible = true,
        Infravision = true,
        Mindless = true
    };
}
