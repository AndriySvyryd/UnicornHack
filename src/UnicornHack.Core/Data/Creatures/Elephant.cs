namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Elephant = new Creature
    {
        Name = "elephant",
        SpeciesClass = SpeciesClass.Quadrupedal,
        Size = 8,
        Weight = 3800,
        PerceptionBonus = 1,
        SpeedBonus = 1,
        Armor = 2,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "tusk sweep",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Headbutt,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "180*MightModifier()" } }
            },
            new Ability
            {
                Name = "driving charge",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Headbutt,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "180*MightModifier()" }, new Topple() }
            }
        },
        EnglishDescription = "An enormous, grey-skinned animal with pillar-like legs, broad ears and a long, flexible trunk. Deep folds crease its hide, and curved ivory tusks frame its mouth.",
        InitialLevel = 20,
        GenerationWeight = "0",
        Behavior = AIBehavior.RangedPeaceful | AIBehavior.Wandering | AIBehavior.Displacing,
        Noise = ActorNoiseType.Roar
    };
}
