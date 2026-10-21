namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Spriggan = new Creature
    {
        Name = "spriggan",
        SpeciesClass = SpeciesClass.Fey,
        Weight = 600,
        PerceptionBonus = -8,
        MightBonus = -7,
        SpeedBonus = -5,
        FocusBonus = -5,
        MagicResistance = 10,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        LocomotionType = LocomotionType.Walking | LocomotionType.Teleportation,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "drowsing touch",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Sedate() }
            },
            new Ability
            {
                Name = "stunning touch",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Stun() }
            }
        },
        EnglishDescription = "A gnarled, wiry humanoid with bark-brown skin and a mop of tangled hair. Its knobby fingers resemble thin roots, and its narrow face is creased around bright, deep-set eyes.",
        InitialLevel = 6,
        GenerationWeight = "$branch == 'ruined stronghold' && $depth <= 2 ? 1 : 0",
        Noise = ActorNoiseType.Seduction,
        Sex = Sex.Female
    };
}
