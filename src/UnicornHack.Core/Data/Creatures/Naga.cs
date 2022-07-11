namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Naga = new Creature
    {
        Name = "naga",
        Species = Species.Naga,
        SpeciesClass = SpeciesClass.Aberration,
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
                Effects = new List<Effect> { new PhysicalDamage { Damage = "70*MightModifier()" } }
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
                Effects = new List<Effect> { new Corrode { Damage = "70*MightModifier()" } }
            }
        },
        InitialLevel = 8,
        NextStageName = "greater naga",
        Noise = ActorNoiseType.Hiss,
        Size = 16,
        Weight = 1500,
        MovementDelay = -15,
        TurningDelay = -15,
        Perception = -5,
        Might = -6,
        Speed = -5,
        Focus = -6,
        Armor = 4,
        MagicResistance = 5,
        AcidResistance = 75,
        StoningImmune = true,
        TorsoType = TorsoType.Serpentine,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        SlotCapacity = 1,
        Infravision = true
    };
}
