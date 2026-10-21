namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Draugr = new Creature
    {
        Name = "draugr",
        SpeciesClass = SpeciesClass.Undead,
        Weight = 1200,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -4,
        HealingResistance = 150,
        Armor = 2,
        MagicResistance = 2,
        ColdResistance = 75,
        SuffocateResistance = 100,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        Infravision = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "grave strength",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            },
            new Ability
            {
                Name = "mound-keeper fist",
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
        EnglishDescription = "A stiff, heavy-bodied corpse with dark, leathery skin stretched over a swollen frame. Faded burial cloth clings to its shoulders, and earth fills the creases of its rigid hands.",
        InitialLevel = 3,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.Stalking,
        Noise = ActorNoiseType.Howl
    };
}
