namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Wolf = new Creature
    {
        Name = "wolf",
        SpeciesClass = SpeciesClass.Canine,
        Weight = 1200,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -6,
        Armor = 3,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "hamstringing bite",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect>
                {
                    new PhysicalDamage { Amount = "70*MightModifier()" },
                    new BleedDamage { Duration = EffectDuration.UntilTimeout, Amount = "20*MightModifier()" }
                }
            },
            new Ability
            {
                Name = "pack harry",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 300,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "30*MightModifier()" }, new Topple() }
            }
        },
        EnglishDescription = "A lean canine with a thick grey coat, erect ears and a long, narrow muzzle. Its strong legs end in broad paws, and a bushy tail hangs below its straight back.",
        InitialLevel = 7,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.SmallGroup,
        Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
        Noise = ActorNoiseType.Bark
    };
}
