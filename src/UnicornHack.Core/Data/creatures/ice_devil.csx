new Creature
{
    Name = "ice devil",
    Species = Species.DemonMajor,
    SpeciesClass = SpeciesClass.Demon,
    ArmorClass = -4,
    MagicResistance = 55,
    MovementDelay = 200,
    Weight = 1800,
    Size = Size.Large,
    Nutrition = 400,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Sting,
            Timeout = 1,
            Effects = new HashSet<Effect> { new ColdDamage { Damage = 7 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 4 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Infravision", "InvisibilityDetection", "Infravisibility", "Humanoidness", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    InitialLevel = 11,
    PreviousStageName = "bone devil",
    CorpseName = "",
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.HellOnly,
    GenerationFrequency = Frequency.Uncommonly,
    Behavior = MonsterBehavior.Stalking,
    Alignment = -12
}
