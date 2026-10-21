namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Rakshasa = new Creature
    {
        Name = "rakshasa",
        SpeciesClass = SpeciesClass.Humanoid,
        Weight = 1200,
        PerceptionBonus = -7,
        MightBonus = -8,
        SpeedBonus = -7,
        FocusBonus = -4,
        Armor = 1,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        SlotCapacity = 3,
        Infravisible = true,
        Infravision = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "practiced force",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "aimed shot",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Shoot,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*PerceptionModifier()" } }
            },
            new Ability
            {
                Name = "close-quarters strike",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage() }
            },
            new Ability
            {
                Name = "wall-breaking punch",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "60*MightModifier()" } }
            }
        },
        EnglishDescription = "A tall, broad-shouldered humanoid with tawny, striped fur and a feline face. Pointed ears rise above a heavy brow, and long whiskers frame its strong muzzle.",
        InitialLevel = 5,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.SmallGroup,
        Noise = ActorNoiseType.Speach
    };
}
