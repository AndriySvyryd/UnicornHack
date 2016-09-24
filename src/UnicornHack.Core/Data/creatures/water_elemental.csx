new CreatureVariant
{
    InitialLevel = 8,
    ArmorClass = 2,
    MagicResistance = 30,
    GenerationFrequency = Frequency.Uncommonly,
    CorpseVariantName = "",
    Name = "water elemental",
    Species = Species.Elemental,
    SpeciesClass = SpeciesClass.Extraplanar,
    MovementRate = 6,
    Size = Size.Huge,
    Weight = 2500,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Swimming",
        "NonAnimal",
        "NonSolidBody",
        "Breathlessness",
        "Limblessness",
        "Eyelessness",
        "Headlessness",
        "Mindlessness",
        "Asexuality"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "StoningResistance", 3 }, { "SlimingResistance", 3 }, { "SicknessResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 17 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new WaterDamage { Damage = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new WaterDamage { Damage = 3 } } }
    }
}
