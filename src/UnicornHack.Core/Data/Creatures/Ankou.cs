namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Ankou = new Creature
    {
        Name = "Ankou",
        SpeciesClass = SpeciesClass.Humanoid,
        Weight = 800,
        PerceptionBonus = 2,
        MightBonus = 4,
        FocusBonus = 2,
        Armor = 2,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new LeveledAbility
            {
                Name = "Ankou trophies",
                Type = AbilityType.Buff,
                LevelExpression = "1 + Min(3, $levelDeaths >> 1)",
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
                                    Name = "reverse scythe",
                                    Type = AbilityType.MeleeAttack,
                                    Windup = 100,
                                    Activation = ActivationType.Targeted,
                                    Trigger = ActivationType.OnMeleeAttack,
                                    MinHeadingDeviation = 4,
                                    MaxHeadingDeviation = 4,
                                    Range = 1,
                                    TargetingShape = TargetingShape.Cone,
                                    Action = AbilityAction.Slash,
                                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                                    Accuracy = "5+PerceptionModifier()",
                                    Cooldown = 200,
                                    Delay = "100*SpeedModifier()",
                                    Effects = new List<Effect> { new PhysicalDamage { Amount = "100*MightModifier()" } }
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
                                    Name = "reverse scythe",
                                    Type = AbilityType.MeleeAttack,
                                    Windup = 100,
                                    Activation = ActivationType.Targeted,
                                    Trigger = ActivationType.OnMeleeAttack,
                                    MinHeadingDeviation = 4,
                                    MaxHeadingDeviation = 4,
                                    Range = 1,
                                    TargetingShape = TargetingShape.Cone,
                                    Action = AbilityAction.Slash,
                                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                                    Accuracy = "5+PerceptionModifier()",
                                    Cooldown = 200,
                                    Delay = "100*SpeedModifier()",
                                    Effects = new List<Effect> { new PhysicalDamage { Amount = "100*MightModifier()+2" } }
                                }
                            }
                        }
                    },
                    {
                        3,
                        new List<Effect>
                        {
                            new AddAbility
                            {
                                TargetActivator = true,
                                Ability = new Ability
                                {
                                    Name = "extended reverse scythe",
                                    Type = AbilityType.MeleeAttack,
                                    Windup = 100,
                                    Activation = ActivationType.Targeted,
                                    Trigger = ActivationType.OnMeleeAttack,
                                    MinHeadingDeviation = 4,
                                    MaxHeadingDeviation = 4,
                                    Range = 2,
                                    TargetingShape = TargetingShape.Cone,
                                    Action = AbilityAction.Slash,
                                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                                    Accuracy = "5+PerceptionModifier()",
                                    Cooldown = 200,
                                    Delay = "100*SpeedModifier()",
                                    Effects = new List<Effect> { new PhysicalDamage { Amount = "100*MightModifier()+4" } }
                                }
                            }
                        }
                    },
                    {
                        4,
                        new List<Effect>
                        {
                            new AddAbility
                            {
                                TargetActivator = true,
                                Ability = new Ability
                                {
                                    Name = "extended reverse scythe",
                                    Type = AbilityType.MeleeAttack,
                                    Windup = 100,
                                    Activation = ActivationType.Targeted,
                                    Trigger = ActivationType.OnMeleeAttack,
                                    MinHeadingDeviation = 4,
                                    MaxHeadingDeviation = 4,
                                    Range = 2,
                                    TargetingShape = TargetingShape.Cone,
                                    Action = AbilityAction.Slash,
                                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                                    Accuracy = "5+PerceptionModifier()",
                                    Cooldown = 200,
                                    Delay = "100*SpeedModifier()",
                                    Effects = new List<Effect> { new PhysicalDamage { Amount = "100*MightModifier()+6" } }
                                }
                            },
                            new AddAbility
                            {
                                TargetActivator = true,
                                Ability = new Ability
                                {
                                    Name = "trophy armor",
                                    Type = AbilityType.Buff,
                                    Effects = new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "2", PropertyName = "Armor" } }
                                }
                            }
                        }
                    }
                }
            }
        },
        EnglishDescription = "A gaunt cart-driver carries a long scythe with its cutting edge turned behind him. Trophies gather on the cart as other creatures die, and his backward sweep grows longer and heavier.",
        InitialLevel = 8,
        GenerationWeight = "$branch == 'ruined stronghold' && $depth >= 2 ? 0.1 : 0",
        Behavior = AIBehavior.Roaming
    };
}
