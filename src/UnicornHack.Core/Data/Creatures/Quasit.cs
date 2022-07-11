namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Quasit = new Creature
    {
        Name = "quasit",
        Species = Species.Imp,
        SpeciesClass = SpeciesClass.Demon,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Damage = "20*MightModifier()" } }
            },
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ChangeProperty<int> { Duration = EffectDuration.UntilTimeout, DurationAmount = "5", PropertyName = "Speed", Value = -1 } }
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
        Perception = -8,
        Might = -8,
        Speed = -8,
        Focus = -8,
        Regeneration = 3,
        Armor = 4,
        MagicResistance = 10,
        Infravisible = true,
        Infravision = true
    };
}
