namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Werewolf = new Creature
    {
        Name = "werewolf",
        SpeciesClass = SpeciesClass.ShapeChanger,
        Weight = 1000,
        PerceptionBonus = -7,
        MightBonus = -8,
        SpeedBonus = -7,
        FocusBonus = -8,
        Regeneration = 3,
        MagicResistance = 10,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "savage force",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "rending strike",
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
            },
            new Ability
            {
                Name = "infecting bite",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 250,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ConferLycanthropy { CreatureName = "werewolf" } }
            }
        },
        EnglishDescription = "A tall, fur-covered humanoid with a wolf's head, hunched shoulders and long, muscular arms. Its pointed ears stand above a ridged muzzle, and coarse hair thickens along its back and forearms.",
        InitialLevel = 5,
        GenerationWeight = "$branch == 'ruined stronghold' && $depth >= 2 ? 1 : 0",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Noise = ActorNoiseType.Lycanthrope,
        Lycanthropy = "werewolf"
    };
}
