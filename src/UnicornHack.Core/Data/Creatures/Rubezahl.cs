namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Rubezahl = new Creature
    {
        Name = "Rubezahl",
        SpeciesClass = SpeciesClass.Humanoid,
        Size = 4,
        Weight = 4000,
        PerceptionBonus = 5,
        MightBonus = 8,
        SpeedBonus = 2,
        FocusBonus = 6,
        Armor = 4,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new LeveledAbility
            {
                Name = "Rubezahl's stone cycle",
                Type = AbilityType.Buff,
                LevelExpression = "$hitPoints <= $hitPointMaximum / 2 ? 2 : 1",
                NonDecreasingLevel = true,
                Cumulative = false,
                LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
                {
                    {
                        1,
                        new List<Effect>
                        {
                            new AddAbility
                            {
                                TargetActivator = true,
                                Ability = new Ability
                                {
                                    Name = "mountain blow",
                                    Type = AbilityType.MeleeAttack,
                                    Activation = ActivationType.Targeted,
                                    Trigger = ActivationType.OnMeleeAttack,
                                    Range = 1,
                                    Action = AbilityAction.Punch,
                                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                                    Accuracy = "5+PerceptionModifier()",
                                    Cooldown = 100,
                                    Delay = "100*SpeedModifier()",
                                    Effects = new List<Effect> { new PhysicalDamage() }
                                }
                            }
                        }
                    },
                    {
                        2,
                        new List<Effect>
                        {
                            new AddAbility
                            {
                                TargetActivator = true,
                                Ability = new Ability
                                {
                                    Name = "cast stone",
                                    Type = AbilityType.RangedAttack,
                                    Windup = 100,
                                    Activation = ActivationType.Targeted,
                                    Trigger = ActivationType.OnRangedAttack,
                                    Range = 6,
                                    Action = AbilityAction.Throw,
                                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                                    Accuracy = "5+PerceptionModifier()",
                                    Cooldown = 300,
                                    Delay = "100*SpeedModifier()",
                                    Effects = new List<Effect> { new PhysicalDamage() }
                                }
                            }
                        }
                    }
                }
            }
        },
        EnglishDescription = "A tall, rugged figure with a tangled beard, heavy brows and broad, stone-grey hands. A weathered cloak hangs in uneven folds over a thickset frame.",
        InitialLevel = 10,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.BranchBoss
    };
}
