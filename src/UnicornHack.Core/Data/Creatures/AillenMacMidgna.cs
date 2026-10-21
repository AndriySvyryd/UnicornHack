namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature AillenMacMidgna = new Creature
    {
        Name = "Aillen mac Midgna",
        SpeciesClass = SpeciesClass.Humanoid,
        Weight = 800,
        PerceptionBonus = 2,
        MightBonus = 1,
        FocusBonus = 4,
        Armor = 1,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new LeveledAbility
            {
                Name = "sleeping air and candle",
                Type = AbilityType.Buff,
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
                                    Name = "sleeping air",
                                    Type = AbilityType.RangedAttack,
                                    Windup = 100,
                                    Activation = ActivationType.Targeted,
                                    Trigger = ActivationType.OnRangedAttack,
                                    Range = 5,
                                    Action = AbilityAction.Spell,
                                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                                    Accuracy = "5+PerceptionModifier()",
                                    Delay = "100*SpeedModifier()",
                                    EnergyPointCost = 5,
                                    Effects = new List<Effect> { new Sedate() }
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
                                    Name = "raised candle",
                                    Type = AbilityType.RangedAttack,
                                    Windup = 100,
                                    Activation = ActivationType.Targeted,
                                    Trigger = ActivationType.OnRangedAttack,
                                    Range = 5,
                                    Action = AbilityAction.Spell,
                                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                                    Accuracy = "5+PerceptionModifier()",
                                    Cooldown = 200,
                                    Delay = "100*SpeedModifier()",
                                    EnergyPointCost = 8,
                                    Effects = new List<Effect> { new FireDamage { Amount = "80*FocusModifier()" } }
                                }
                            }
                        }
                    }
                }
            }
        },
        EnglishDescription = "A narrow-faced musician carries a small instrument and a candle in a sheltered hand. He lowers his head to play a slow air, then lifts the flame before releasing a bright wash of fire.",
        InitialLevel = 7,
        GenerationWeight = "$branch == 'ruined stronghold' && $depth == 2 ? 0.15 : 0"
    };
}
