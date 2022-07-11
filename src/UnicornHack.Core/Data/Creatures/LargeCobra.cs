namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature LargeCobra = new Creature
    {
        Name = "large cobra",
        Species = Species.Snake,
        SpeciesClass = SpeciesClass.Reptile,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Blight { Damage = "50*MightModifier()" } }
            },
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Spit,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Blind { Duration = EffectDuration.UntilTimeout, DurationAmount = "5" } }
            }
        },
        InitialLevel = 7,
        GenerationWeight = "2",
        Noise = ActorNoiseType.Hiss,
        Weight = 250,
        MovementDelay = -34,
        TurningDelay = -34,
        Perception = -6,
        Might = -6,
        Speed = -6,
        Focus = -6,
        Armor = 4,
        TorsoType = TorsoType.Serpentine,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        LocomotionType = LocomotionType.Swimming,
        SlotCapacity = 0,
        VisibilityLevel = 2,
        Infravision = true
    };
}
