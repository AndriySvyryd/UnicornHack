namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature CentaurWarrior = new Creature
    {
        Name = "centaur warrior",
        SpeciesClass = SpeciesClass.Humanoid,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "centaur warrior ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "centaur warrior ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Kick,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "40*MightModifier()" } }
            }
        },
        InitialLevel = 6,
        Behavior = AIBehavior.GoldCollector | AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Speach,
        Size = 8,
        Weight = 2000,
        MovementDelay = -40,
        TurningDelay = -40,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -6,
        Armor = 4,
        MagicResistance = 5,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Hooves,
        Infravisible = true
    };
}
