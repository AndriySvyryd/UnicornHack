namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Gnome = new Creature
    {
        Name = "gnome",
        SpeciesClass = SpeciesClass.Humanoid,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "gnome ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "30*MightModifier()" } }
            },
            new Ability
            {
                Name = "gnome ability #2",
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
        InitialLevel = 1,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable | CreatureGenerationFlags.SmallGroup,
        Behavior = AIBehavior.GoldCollector | AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Speach,
        Size = 2,
        Weight = 650,
        MovementDelay = 100,
        TurningDelay = 100,
        PerceptionBonus = -9,
        MightBonus = -8,
        SpeedBonus = -7,
        FocusBonus = -5,
        MagicResistance = 2,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true
    };
}
