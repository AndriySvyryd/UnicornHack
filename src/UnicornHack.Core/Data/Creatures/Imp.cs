namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Imp = new Creature
    {
        Name = "imp",
        SpeciesClass = SpeciesClass.Demon,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "imp ability #1",
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
            }
        },
        InitialLevel = 3,
        GenerationWeight = "5",
        Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
        Noise = ActorNoiseType.Cuss,
        Size = 1,
        Weight = 100,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        Regeneration = 3,
        Armor = 4,
        MagicResistance = 10,
        LocomotionType = LocomotionType.Flying,
        Infravisible = true,
        Infravision = true
    };
}
