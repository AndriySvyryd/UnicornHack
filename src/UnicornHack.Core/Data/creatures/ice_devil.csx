new CreatureVariant
{
    InitialLevel = 11,
    ArmorClass = -4,
    MagicResistance = 55,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.HellOnly,
    GenerationFrequency = Frequency.Uncommonly,
    Behavior = MonsterBehavior.Stalking,
    Alignment = -12,
    CorpseVariantName = "",
    PreviousStageName = "bone devil",
    Name = "ice devil",
    Species = Species.DemonMajor,
    SpeciesClass = SpeciesClass.Demon,
    MovementRate = 6,
    Size = Size.Large,
    Weight = 1800,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Infravision", "InvisibilityDetection", "Infravisibility", "Humanoidness" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "SicknessResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Sting,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ColdDamage { Damage = 7 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 4 } } }
    }
}
