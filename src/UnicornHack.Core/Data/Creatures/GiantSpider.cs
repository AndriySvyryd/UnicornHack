namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature GiantSpider = new Creature
    {
        Name = "giant spider",
        Species = Species.Spider,
        SpeciesClass = SpeciesClass.Vermin,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Name = "posion bite",
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
                Name = "weakening bite",
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ChangeProperty<int> { Duration = EffectDuration.UntilTimeout, DurationAmount = "5", PropertyName = "Might", Value = -1 } }
            }
        },
        InitialLevel = 1,
        GenerationWeight = "2",
        Weight = 150,
        MovementDelay = -20,
        TurningDelay = -20,
        Perception = -7,
        Might = -8,
        Speed = -7,
        Focus = -8,
        Armor = 3,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        Clingy = true
    };
}
