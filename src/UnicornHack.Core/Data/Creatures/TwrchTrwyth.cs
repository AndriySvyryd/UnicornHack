namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature TwrchTrwyth = new Creature
    {
        Name = "Twrch Trwyth",
        SpeciesClass = SpeciesClass.Quadrupedal,
        Size = 3,
        Weight = 1800,
        MightBonus = 2,
        SpeedBonus = -2,
        Armor = 1,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Hooves,
        LocomotionType = LocomotionType.Walking,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "tusk sweep",
                Type = AbilityType.MeleeAttack,
                Windup = 100,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Stab,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage() }
            }
        },
        EnglishDescription = "A massive, dark-bristled boar with a high shoulder ridge and a stiff mane along its spine. Two pale, curved tusks jut from its broad muzzle above thick, cloven hooves.",
        InitialLevel = 2,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.LevelBoss,
        Noise = ActorNoiseType.Grunt
    };
}
