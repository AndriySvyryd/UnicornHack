namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature FrostGiant = new Creature
    {
        Name = "frost giant",
        SpeciesClass = SpeciesClass.Humanoid,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "frost giant ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "110*MightModifier()" } }
            },
            new Ability
            {
                Name = "frost giant ability #2",
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
            }
        },
        InitialLevel = 10,
        GenerationWeight = "$branch == 'hell' ? 0 : 1",
        GenerationFlags = CreatureGenerationFlags.SmallGroup,
        Behavior = AIBehavior.GemCollector | AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Boast,
        Size = 16,
        Weight = 2250,
        PerceptionBonus = -4,
        MightBonus = -4,
        SpeedBonus = -4,
        FocusBonus = -4,
        Armor = 3,
        MagicResistance = 5,
        ColdResistance = 75,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true
    };
}
