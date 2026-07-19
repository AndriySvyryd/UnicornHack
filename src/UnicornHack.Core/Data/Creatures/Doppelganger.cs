namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Doppelganger = new Creature
    {
        Name = "doppelganger",
        SpeciesClass = SpeciesClass.ShapeChanger,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "doppelganger ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "60*MightModifier()" }, new Polymorph { DurationAmount = "300*FocusModifier()", CreatureName = "gnome" } }
            },
            new Ability
            {
                Name = "doppelganger ability #2",
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
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Imitate,
        Weight = 1000,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        Armor = 2,
        MagicResistance = 10,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true
    };
}
