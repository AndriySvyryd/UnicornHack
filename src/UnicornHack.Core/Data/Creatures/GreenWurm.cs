namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature GreenWurm = new Creature
    {
        Name = "green wurm",
        SpeciesClass = SpeciesClass.Reptile,
        MovementDelay = 33,
        TurningDelay = 33,
        Size = 32,
        Weight = 4500,
        PerceptionBonus = -2,
        MightBonus = -2,
        SpeedBonus = -2,
        FocusBonus = -2,
        Armor = 5,
        MagicResistance = 10,
        DecayResistance = 50,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        LocomotionType = LocomotionType.Walking,
        SlotCapacity = 1,
        Infravision = true,
        InvisibilityDetection = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "venom breath",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Breath,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ToxinDamage { Duration = EffectDuration.UntilTimeout, Amount = "140*FocusModifier()" } }
            },
            new Ability
            {
                Name = "crushing bite",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "90*MightModifier()" } }
            },
            new Ability
            {
                Name = "left claw",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "right claw",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            }
        },
        EnglishDescription = "A long-bodied reptile covered in overlapping green scales, darker along its ridged back. Clawed legs brace its low body, and a thick tail curves behind its narrow, angular head.",
        InitialLevel = 15,
        GenerationWeight = "$branch == 'ruined stronghold' && $depth == 3 ? 2 : 0",
        Behavior = AIBehavior.Mountable,
        Noise = ActorNoiseType.Roar
    };
}
