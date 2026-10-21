namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Bugbear = new Creature
    {
        Name = "bugbear",
        SpeciesClass = SpeciesClass.Humanoid,
        MovementDelay = 33,
        TurningDelay = 33,
        Size = 8,
        Weight = 1250,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        Armor = 2,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "ambush force",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "looming punch",
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
                Name = "terrifying scream",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 8,
                Action = AbilityAction.Scream,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 300,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Taunt() }
            }
        },
        EnglishDescription = "A broad, shaggy humanoid with a thick neck, a blunt muzzle and heavy arms. Coarse hair bristles around its ears and spills over the edges of its rough clothing.",
        InitialLevel = 5,
        GenerationWeight = "$branch == 'ruined stronghold' ? 4 : 0",
        Noise = ActorNoiseType.Growl
    };
}
