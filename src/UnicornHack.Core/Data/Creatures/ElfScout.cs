namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature ElfScout = new Creature
    {
        Name = "elf scout",
        SpeciesClass = SpeciesClass.Humanoid,
        Size = 6,
        Weight = 700,
        PerceptionBonus = 2,
        SpeedBonus = 2,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "elf-shot",
                Type = AbilityType.RangedAttack,
                Windup = 100,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 5,
                Action = AbilityAction.Shoot,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 200,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "25*MightModifier()", ArmorPenetration = "3" }, new Cripple() }
            }
        },
        EnglishDescription = "A slight, pointed-eared figure with sharp features and watchful eyes. Close-fitting, travel-worn clothing follows its lean frame as it moves with light, deliberate steps.",
        InitialLevel = 3,
        GenerationWeight = "$branch == 'ruined stronghold' ? ($depth >= 2 ? 1 : 0) : 0"
    };
}
