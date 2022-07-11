namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Succubus = new Creature
    {
        Name = "succubus",
        Species = Species.Succubus,
        SpeciesClass = SpeciesClass.Demon,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Sedate { Duration = EffectDuration.UntilTimeout, DurationAmount = "5" } }
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
                Effects = new List<Effect> { new PhysicalDamage { Damage = "20*MightModifier()" } }
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
                Effects = new List<Effect> { new PhysicalDamage { Damage = "20*MightModifier()" } }
            }
        },
        InitialLevel = 6,
        GenerationWeight = "$branch == 'hell' ? 2 : 0",
        GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.Stalking | AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Seduction,
        Weight = 1400,
        Perception = -6,
        Might = -6,
        Speed = -6,
        Focus = -6,
        MagicResistance = 35,
        FireResistance = 75,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        LocomotionType = LocomotionType.Flying,
        Infravisible = true,
        Infravision = true
    };
}
