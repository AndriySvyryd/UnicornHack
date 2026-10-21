namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Boggart = new Creature
    {
        Name = "boggart",
        SpeciesClass = SpeciesClass.Humanoid,
        MovementDelay = 100,
        TurningDelay = 100,
        Size = 2,
        Weight = 400,
        PerceptionBonus = -9,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -9,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "heavy blows",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            },
            new Ability
            {
                Name = "bruising punch",
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
        EnglishDescription = "A wiry, hunched humanoid with coarse skin, uneven ears and a broad, wrinkled nose. Its long fingers hang below ragged cuffs, twitching as it shifts from foot to foot.",
        InitialLevel = 1,
        GenerationWeight = "$branch == 'outer wards' ? 2 : ($branch == 'ruined stronghold' ? 3 : 0)",
        Noise = ActorNoiseType.Grunt
    };
}
