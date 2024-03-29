namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Abollar = new Creature
    {
        Name = "abollar",
        Species = Species.Abollar,
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
            },
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Suck,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ChangeProperty<int> { Duration = EffectDuration.UntilTimeout, DurationAmount = "10", PropertyName = "Focus", Value = -2 } }
            }
        },
        InitialLevel = 21,
        GenerationWeight = "4.0",
        Behavior = AIBehavior.GoldCollector | AIBehavior.GemCollector | AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Gurgle,
        Size = 8,
        Weight = 1200,
        Perception = -5,
        Might = -6,
        Speed = -5,
        Focus = -6,
        Armor = 2,
        MagicResistance = 40,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        LocomotionType = LocomotionType.Flying,
        Telepathic = 3,
        Infravisible = true,
        Infravision = true,
        InvisibilityDetection = true
    };
}
