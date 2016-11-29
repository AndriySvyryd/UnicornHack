new CreatureVariant
{
    InitialLevel = 5,
    GenerationFrequency = Frequency.Sometimes,
    CorpseVariantName = "",
    Name = "black light",
    Species = Species.FloatingSphere,
    SpeciesClass = SpeciesClass.Extraplanar,
    MovementRate = 15,
    Size = Size.Small,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Flight",
        "FlightControl",
        "Infravisibility",
        "InvisibilityDetection",
        "NonAnimal",
        "NonSolidBody",
        "Breathlessness",
        "Limblessness",
        "Eyelessness",
        "Headlessness",
        "Mindlessness",
        "Asexuality",
        "NoInventory"
    }
,
    ValuedProperties = new Dictionary<string, Object>
    {
        {
            "FireResistance",
            3
        },
        {
            "ColdResistance",
            3
        },
        {
            "ElectricityResistance",
            3
        },
        {
            "AcidResistance",
            3
        },
        {
            "DisintegrationResistance",
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
            "SlimingResistance",
            3
        },
        {
            "SicknessResistance",
            3
        },
        {
            "Stealthiness",
            3
        }
    }
,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Explosion,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Hallucinate { Duration = 27 } }
        }
    }
}
