new CreatureVariant
{
    InitialLevel = 14,
    ArmorClass = 4,
    MagicResistance = 50,
    GenerationFrequency = Frequency.Rarely,
    CorpseVariantName = "",
    Name = "stone golem",
    Species = Species.Golem,
    MovementRate = 6,
    Size = Size.Large,
    Weight = 2000,
    SimpleProperties = new HashSet<string> { "SleepResistance", "NonAnimal", "Breathlessness", "Mindlessness", "Humanoidness", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object>
    {
        {
            "ColdResistance",
            3
        },
        {
            "FireResistance",
            3
        },
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
            "StoningResistance",
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
            60
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
