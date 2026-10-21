namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature CuSidhe = new Creature
    {
        Name = "cu sidhe",
        SpeciesClass = SpeciesClass.Canine,
        Weight = 1600,
        PerceptionBonus = 2,
        MightBonus = 2,
        SpeedBonus = 2,
        Armor = 2,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new LeveledAbility
            {
                Name = "three bays",
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
                                    Name = "first bay",
                                    Type = AbilityType.RangedAttack,
                                    Windup = 100,
                                    Activation = ActivationType.Targeted,
                                    Trigger = ActivationType.OnRangedAttack,
                                    Range = 5,
                                    Action = AbilityAction.Scream,
                                    Delay = "100*SpeedModifier()",
                                    Effects = new List<Effect> { new Frighten { Amount = "0" } }
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
                                    Name = "second bay",
                                    Type = AbilityType.RangedAttack,
                                    Windup = 100,
                                    Activation = ActivationType.Targeted,
                                    Trigger = ActivationType.OnRangedAttack,
                                    Range = 5,
                                    Action = AbilityAction.Scream,
                                    Delay = "100*SpeedModifier()",
                                    Effects = new List<Effect> { new Frighten { Amount = "0" } }
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
                                    Name = "third bay",
                                    Type = AbilityType.RangedAttack,
                                    Windup = 100,
                                    Activation = ActivationType.Targeted,
                                    Trigger = ActivationType.OnRangedAttack,
                                    Range = 5,
                                    Action = AbilityAction.Scream,
                                    Delay = "100*SpeedModifier()",
                                    Effects = new List<Effect> { new Frighten { Amount = "0" } }
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
                                    Name = "baying charge",
                                    Type = AbilityType.MeleeAttack,
                                    Windup = 100,
                                    Activation = ActivationType.Targeted,
                                    Trigger = ActivationType.OnMeleeAttack,
                                    Range = 4,
                                    Action = AbilityAction.Bite,
                                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                                    Accuracy = "5+PerceptionModifier()",
                                    Cooldown = 300,
                                    Delay = "100*SpeedModifier()",
                                    EnergyPointCost = 5,
                                    Effects = new List<Effect> { new Teleport { TargetActivator = true }, new PhysicalDamage { Amount = "70*MightModifier()" }, new Cripple() }
                                }
                            }
                        }
                    }
                }
            }
        },
        EnglishDescription = "A large, dark-coated hound with a broad muzzle and thick paws. Its chest rises before each of three deep, separated bays; the final call gives way to a low, committed rush.",
        InitialLevel = 7,
        GenerationWeight = "$branch == 'ruined stronghold' && $depth >= 2 ? 0.4 : 0",
        Noise = ActorNoiseType.Bark
    };
}
