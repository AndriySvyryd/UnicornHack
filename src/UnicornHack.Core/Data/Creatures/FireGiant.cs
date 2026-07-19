namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature FireGiant = new Creature
    {
        Name = "fire giant",
        SpeciesClass = SpeciesClass.Humanoid,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "fire giant ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "110*MightModifier()" } }
            },
            new Ability
            {
                Name = "fire giant ability #2",
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
        InitialLevel = 9,
        GenerationFlags = CreatureGenerationFlags.SmallGroup,
        Behavior = AIBehavior.GemCollector | AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Boast,
        Size = 16,
        Weight = 2250,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        FocusBonus = -6,
        Armor = 3,
        MagicResistance = 2,
        FireResistance = 75,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true
    };
}
