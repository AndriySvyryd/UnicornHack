namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Quasit = new Creature
    {
        Name = "quasit",
        SpeciesClass = SpeciesClass.Demon,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "quasit ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            },
            new Ability
            {
                Name = "quasit ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect>
                {
                    new ChangeProperty<int> { Duration = EffectDuration.UntilTimeout, DurationAmount = "5*MightModifier()", Amount = "-1", PropertyName = "Speed" }
                }
            }
        },
        InitialLevel = 3,
        GenerationWeight = "5",
        Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
        Noise = ActorNoiseType.Cuss,
        Size = 2,
        Weight = 200,
        MovementDelay = -20,
        TurningDelay = -20,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        Regeneration = 3,
        Armor = 4,
        MagicResistance = 10,
        Infravisible = true,
        Infravision = true
    };
}
