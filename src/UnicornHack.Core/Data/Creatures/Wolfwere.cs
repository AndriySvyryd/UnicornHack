namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Wolfwere = new Creature
    {
        Name = "wolfwere",
        Species = Species.Wolf,
        SpeciesClass = SpeciesClass.Canine | SpeciesClass.ShapeChanger,
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
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 250,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ConferLycanthropy { VariantName = "wolfwere" } }
            }
        },
        InitialLevel = 5,
        GenerationWeight = "0",
        GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
        Noise = ActorNoiseType.Bark,
        Weight = 500,
        Perception = -7,
        Might = -8,
        Speed = -7,
        Focus = -8,
        Regeneration = 3,
        Armor = 3,
        MagicResistance = 10,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true
    };
}
