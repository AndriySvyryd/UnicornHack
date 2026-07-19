namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Unicorn = new Creature
    {
        Name = "unicorn",
        SpeciesClass = SpeciesClass.Quadrupedal,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "unicorn ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Headbutt,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "60*MightModifier()" } }
            },
            new Ability
            {
                Name = "unicorn ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Kick,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "30*MightModifier()" } }
            }
        },
        InitialLevel = 4,
        GenerationWeight = "5",
        Behavior = AIBehavior.AlignmentAware | AIBehavior.RangedPeaceful | AIBehavior.Wandering | AIBehavior.GemCollector,
        Noise = ActorNoiseType.Neigh,
        Size = 8,
        Weight = 1300,
        MovementDelay = -50,
        TurningDelay = -50,
        PerceptionBonus = -7,
        MightBonus = -8,
        SpeedBonus = -7,
        FocusBonus = -8,
        Armor = 4,
        MagicResistance = 35,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        Infravisible = true
    };
}
