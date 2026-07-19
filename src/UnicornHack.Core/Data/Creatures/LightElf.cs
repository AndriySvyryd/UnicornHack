namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature LightElf = new Creature
    {
        Name = "light elf",
        SpeciesClass = SpeciesClass.Humanoid,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "light elf ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "light elf ability #2",
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
        InitialLevel = 5,
        GenerationFlags = CreatureGenerationFlags.SmallGroup,
        Behavior = AIBehavior.AlignmentAware | AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Speach,
        Weight = 800,
        PerceptionBonus = -7,
        MightBonus = -8,
        SpeedBonus = -7,
        FocusBonus = -2,
        MagicResistance = 5,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true,
        InvisibilityDetection = true
    };
}
