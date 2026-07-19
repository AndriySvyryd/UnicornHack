namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Mastodon = new Creature
    {
        Name = "mastodon",
        SpeciesClass = SpeciesClass.Quadrupedal,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "mastodon ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Headbutt,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "180*MightModifier()" } }
            },
            new Ability
            {
                Name = "mastodon ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Headbutt,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "180*MightModifier()" } }
            }
        },
        InitialLevel = 20,
        GenerationWeight = "2",
        Noise = ActorNoiseType.Roar,
        Size = 8,
        Weight = 3800,
        PerceptionBonus = 1,
        SpeedBonus = 1,
        Armor = 2,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true
    };
}
