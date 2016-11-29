new CreatureVariant
{
    InitialLevel = 11,
    ArmorClass = 7,
    MagicResistance = 40,
    GenerationFrequency = Frequency.Rarely,
    CorpseVariantName = "",
    Name = "clay golem",
    Species = Species.Golem,
    MovementRate = 7,
    Size = Size.Large,
    Weight = 1500,
    SimpleProperties = new HashSet<string> { "SleepResistance", "NonAnimal", "Breathlessness", "Mindlessness", "Humanoidness", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object>
    {
        {
            "ElectricityResistance",
            3
        },
        {
            "PoisonResistance",
            3
        },
        {
            "VenomResistance",
            3
        },
        {
            "SicknessResistance",
            3
        },
        {
            "SlimingResistance",
            3
        },
        {
            "ThickHide",
            3
        },
        {
            "MaxHP",
            50
        }
    }
,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 16 } }
        }
    }
}
