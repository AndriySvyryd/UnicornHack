namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Doppelganger = new Creature
    {
        Name = "doppelganger",
        SpeciesClass = SpeciesClass.ShapeChanger,
        Weight = 1000,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        Armor = 2,
        MagicResistance = 10,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "stolen form",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "60*MightModifier()" }, new Polymorph { DurationAmount = "300*FocusModifier()", CreatureName = "rat" } }
            },
            new Ability
            {
                Name = "mimicked punch",
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
        EnglishDescription = "A lean humanoid with smooth, pale skin and curiously unfinished features. Its face has shallow contours, and its long hands mirror one another with unsettling precision.",
        InitialLevel = 9,
        GenerationWeight = "$branch == 'ruined stronghold' && $depth >= 2 ? 1 : 0",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Noise = ActorNoiseType.Imitate
    };
}
