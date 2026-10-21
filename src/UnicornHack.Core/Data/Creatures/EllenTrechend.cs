namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature EllenTrechend = new Creature
    {
        Name = "Ellen Trechend",
        SpeciesClass = SpeciesClass.Bird,
        Size = 4,
        Weight = 2500,
        PerceptionBonus = 2,
        MightBonus = 4,
        SpeedBonus = 2,
        FocusBonus = 4,
        Armor = 2,
        TorsoType = TorsoType.Avian,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new LeveledAbility
            {
                Name = "Ellen Trechend's shriek cycle",
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
                                    Name = "piercing peck",
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
                                    Name = "three-headed shriek",
                                    Type = AbilityType.RangedAttack,
                                    Windup = 100,
                                    Activation = ActivationType.Targeted,
                                    Trigger = ActivationType.OnRangedAttack,
                                    Range = 5,
                                    Action = AbilityAction.Screech,
                                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                                    Accuracy = "5+PerceptionModifier()",
                                    Cooldown = 300,
                                    Delay = "100*SpeedModifier()",
                                    Effects = new List<Effect> { new Deafen() }
                                }
                            }
                        }
                    }
                }
            }
        },
        EnglishDescription = "A massive birdlike creature with three long necks rising from a thick, feathered body. Each head bears a narrow beak, and heavy clawed feet grip the ground beneath it.",
        InitialLevel = 5,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.LevelBoss
    };
}
