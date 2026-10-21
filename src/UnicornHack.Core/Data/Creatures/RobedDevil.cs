namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature RobedDevil = new Creature
    {
        Name = "Robed Devil",
        SpeciesClass = SpeciesClass.Demon,
        Size = 8,
        Weight = 1000,
        PerceptionBonus = -5,
        MightBonus = -7,
        SpeedBonus = -6,
        FocusBonus = -5,
        MagicResistance = 20,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "sabotaging touch",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            }
        },
        EnglishDescription = "A tall, spare figure wrapped in a dark robe that pools around its feet. Small horns break the line of its hood, and a thin, angular face peers from the shadow beneath it.",
        InitialLevel = 8,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.NonRecruitable | AIBehavior.Stalking,
        Noise = ActorNoiseType.Mumble,
        Sex = Sex.None
    };
}
