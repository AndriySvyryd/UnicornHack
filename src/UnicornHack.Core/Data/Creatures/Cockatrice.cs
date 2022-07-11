namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Cockatrice = new Creature
    {
        Name = "cockatrice",
        Species = Species.Cockatrice,
        SpeciesClass = SpeciesClass.MagicalBeast,
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
                Effects = new List<Effect> { new PhysicalDamage { Damage = "20*MightModifier()" } }
            },
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
                Effects = new List<Effect> { new Stone() }
            },
            new Ability
            {
                Activation = ActivationType.OnMeleeHit,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                Accuracy = "10+PerceptionModifier()",
                Effects = new List<Effect> { new Stone() }
            }
        },
        InitialLevel = 5,
        GenerationFlags = GenerationFlags.SmallGroup,
        Noise = ActorNoiseType.Hiss,
        Size = 2,
        Weight = 30,
        MovementDelay = 100,
        TurningDelay = 100,
        Perception = -7,
        Might = -8,
        Speed = -7,
        Focus = -8,
        Armor = 2,
        MagicResistance = 15,
        StoningImmune = true,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true
    };
}
