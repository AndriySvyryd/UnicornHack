namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Unicorn = new Creature
    {
        Name = "unicorn",
        Species = Species.Unicorn,
        SpeciesClass = SpeciesClass.Quadrupedal,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Headbutt,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Damage = "60*MightModifier()" } }
            },
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Kick,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Damage = "30*MightModifier()" } }
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
        Perception = -7,
        Might = -8,
        Speed = -7,
        Focus = -8,
        Armor = 4,
        MagicResistance = 35,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        Infravisible = true
    };
}
