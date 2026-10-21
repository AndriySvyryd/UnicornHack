namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Imp = new Creature
    {
        Name = "imp",
        SpeciesClass = SpeciesClass.Demon,
        Size = 1,
        Weight = 100,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        Armor = 4,
        MagicResistance = 10,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        SlotCapacity = 2,
        NonAnimal = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "mischievous claw",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            },
            new Ability
            {
                Name = "rope-gnawing bite",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            }
        },
        EnglishDescription = "A small, wiry humanoid with pointed ears, a sharp chin and reddish, finely wrinkled skin. Its thin fingers and oversized feet give its crouched silhouette an awkward, angular look.",
        InitialLevel = 3,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.NonRecruitable | AIBehavior.Wandering | AIBehavior.Stalking,
        Noise = ActorNoiseType.Cuss
    };
}
