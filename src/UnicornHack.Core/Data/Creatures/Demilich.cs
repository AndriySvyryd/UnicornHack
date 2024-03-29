namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Demilich = new Creature
    {
        Name = "demilich",
        Species = Species.Lich,
        SpeciesClass = SpeciesClass.Undead,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Freeze { Damage = "70*MightModifier()" } }
            },
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Spell,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ScriptedEffect { Script = "ArcaneSpell" } }
            }
        },
        InitialLevel = 14,
        PreviousStageName = "lich",
        Behavior = AIBehavior.MagicUser,
        Noise = ActorNoiseType.Mumble,
        Weight = 600,
        MovementDelay = 33,
        TurningDelay = 33,
        Perception = -2,
        Might = -2,
        Speed = -2,
        Focus = 2,
        Regeneration = 3,
        Armor = 6,
        MagicResistance = 30,
        ColdResistance = 75,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        Infravision = true
    };
}
