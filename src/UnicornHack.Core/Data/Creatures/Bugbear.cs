namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Bugbear = new Creature
    {
        Name = "bugbear",
        Species = Species.Bugbear,
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
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Damage = "10*MightModifier()" } }
            }
        },
        InitialLevel = 5,
        GenerationWeight = "4",
        Behavior = AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Growl,
        Size = 8,
        Weight = 1250,
        MovementDelay = 33,
        TurningDelay = 33,
        Perception = -8,
        Might = -8,
        Speed = -8,
        Focus = -8,
        Armor = 2,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true
    };
}
