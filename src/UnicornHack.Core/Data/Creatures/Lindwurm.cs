namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Lindwurm = new Creature
    {
        Name = "Lindwurm",
        SpeciesClass = SpeciesClass.Reptile,
        Size = 4,
        Weight = 3500,
        PerceptionBonus = 2,
        MightBonus = 6,
        FocusBonus = 2,
        Armor = 3,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        LocomotionType = LocomotionType.Walking,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new LeveledAbility
            {
                Name = "Lindwurm's tail cycle",
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
                                    Name = "watery bite",
                                    Type = AbilityType.MeleeAttack,
                                    Activation = ActivationType.Targeted,
                                    Trigger = ActivationType.OnMeleeAttack,
                                    Range = 1,
                                    Action = AbilityAction.Bite,
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
                                    Name = "tail sweep",
                                    Type = AbilityType.MeleeAttack,
                                    Windup = 100,
                                    Activation = ActivationType.Targeted,
                                    Trigger = ActivationType.OnMeleeAttack,
                                    Range = 2,
                                    TargetingShapeSize = 2,
                                    TargetingShape = TargetingShape.Cone,
                                    Action = AbilityAction.Hit,
                                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                                    Accuracy = "5+PerceptionModifier()",
                                    Cooldown = 300,
                                    Delay = "100*SpeedModifier()",
                                    Effects = new List<Effect> { new PhysicalDamage(), new Topple() }
                                }
                            }
                        }
                    }
                }
            }
        },
        EnglishDescription = "A long, low-slung reptilian creature plated in dull, overlapping scales. A broad jaw, heavy clawed feet and a thick, tapering tail give its body a powerful, dragging outline.",
        InitialLevel = 6,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.LevelBoss
    };
}
