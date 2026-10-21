namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature BalorUaNeit = new Creature
    {
        Name = "Balor ua Neit",
        SpeciesClass = SpeciesClass.Humanoid,
        Size = 4,
        Weight = 4000,
        PerceptionBonus = 4,
        MightBonus = 10,
        FocusBonus = 6,
        Armor = 3,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new LeveledAbility
            {
                Name = "Balor's eye cycle",
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
                                    Name = "king's blow",
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
                                    Name = "lifted eye",
                                    Type = AbilityType.RangedAttack,
                                    Windup = 100,
                                    Activation = ActivationType.Targeted,
                                    Trigger = ActivationType.OnRangedAttack,
                                    Range = 8,
                                    Action = AbilityAction.Gaze,
                                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                                    Accuracy = "5+PerceptionModifier()",
                                    Cooldown = 400,
                                    Delay = "100*SpeedModifier()",
                                    EnergyPointCost = 15,
                                    Effects = new List<Effect> { new LightDamage() }
                                }
                            }
                        }
                    }
                }
            }
        },
        EnglishDescription = "A towering, broad-shouldered figure with coarse skin and one immense, heavy-lidded eye. Dark folds hang beneath the lid, and thick hands curl at his sides.",
        InitialLevel = 10,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.BranchBoss
    };
}
