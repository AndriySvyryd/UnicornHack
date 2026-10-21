namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature HellmouthUsher = new Creature
    {
        Name = "hellmouth usher",
        SpeciesClass = SpeciesClass.Demon,
        Weight = 1200,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        FocusBonus = -6,
        MagicResistance = 17,
        FireResistance = 75,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "goad jab",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Stab,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "driving blow",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bludgeon,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "70*MightModifier()" } }
            }
        },
        EnglishDescription = "A long-limbed figure with an angular face and a mouth that stretches into an unnaturally wide smile. A dark, high-collared garment hangs straight from its narrow shoulders.",
        InitialLevel = 8,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.NonRecruitable | AIBehavior.Stalking | AIBehavior.Displacing,
        Noise = ActorNoiseType.Cuss
    };
}
