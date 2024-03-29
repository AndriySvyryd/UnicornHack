namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Boggart = new Creature
    {
        Name = "boggart",
        Species = Species.Boggart,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Damage = "20" } }
            },
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Damage = "10*MightModifier()" } }
            }
        },
        InitialLevel = 1,
        Behavior = AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Grunt,
        Size = 2,
        Weight = 400,
        MovementDelay = 100,
        TurningDelay = 100,
        Perception = -9,
        Might = -8,
        Speed = -8,
        Focus = -9,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true
    };
}
