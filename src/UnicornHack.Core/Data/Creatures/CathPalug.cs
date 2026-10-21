namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature CathPalug = new Creature
    {
        Name = "Cath Palug",
        SpeciesClass = SpeciesClass.Feline,
        Weight = 1200,
        PerceptionBonus = 4,
        MightBonus = 6,
        SpeedBonus = 4,
        Armor = 2,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true,
        Infravision = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "claw attack",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage() }
            }
        },
        EnglishDescription = "An enormous cat with a low, muscular body, coarse dark fur and a long, restless tail. Pale whiskers frame its muzzle, and hooked claws protrude from its broad paws.",
        InitialLevel = 6,
        GenerationWeight = "0",
        Noise = ActorNoiseType.Grunt
    };
}
