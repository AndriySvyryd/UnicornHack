namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature CentaurWarrior = new Creature
    {
        Name = "centaur warrior",
        Species = Species.Centaur,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Damage = "50" } }
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
                Effects = new List<Effect> { new PhysicalDamage { Damage = "40*MightModifier()" } }
            }
        },
        InitialLevel = 6,
        Behavior = AIBehavior.GoldCollector | AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Speach,
        Size = 8,
        Weight = 2000,
        MovementDelay = -40,
        TurningDelay = -40,
        Perception = -6,
        Might = -6,
        Speed = -6,
        Focus = -6,
        Armor = 4,
        MagicResistance = 5,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Hooves,
        Infravisible = true
    };
}
