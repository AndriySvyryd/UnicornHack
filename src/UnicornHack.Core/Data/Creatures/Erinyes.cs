namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Erinyes = new Creature
    {
        Name = "erinyes",
        SpeciesClass = SpeciesClass.Demon,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "erinyes ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "erinyes ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ToxinDamage { Duration = EffectDuration.UntilTimeout, Amount = "10*MightModifier()" } }
            }
        },
        InitialLevel = 7,
        GenerationWeight = "$branch == 'hell' ? 2 : 0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable | CreatureGenerationFlags.SmallGroup,
        Behavior = AIBehavior.Stalking | AIBehavior.WeaponCollector,
        Weight = 1000,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -6,
        Armor = 4,
        MagicResistance = 15,
        FireResistance = 75,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true
    };
}
