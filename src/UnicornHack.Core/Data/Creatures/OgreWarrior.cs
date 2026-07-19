namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature OgreWarrior = new Creature
    {
        Name = "ogre warrior",
        SpeciesClass = SpeciesClass.Humanoid,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "ogre warrior ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "60*MightModifier()" } }
            },
            new Ability
            {
                Name = "ogre warrior ability #2",
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
        MovementDelay = 20,
        TurningDelay = 20,
        Size = 8,
        Weight = 1600,
        PerceptionBonus = -7,
        MightBonus = -8,
        SpeedBonus = -7,
        FocusBonus = -8,
        Armor = 2,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        InitialLevel = 5,
        GenerationWeight = "3",
        NextStageName = "ogre lord",
        GenerationFlags = CreatureGenerationFlags.SmallGroup,
        Behavior = AIBehavior.GoldCollector | AIBehavior.GemCollector | AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Grunt
    };
}
