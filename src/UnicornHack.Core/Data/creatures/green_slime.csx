new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 6,
    GenerationFlags = GenerationFlags.HellOnly,
    GenerationFrequency = Frequency.Rarely,
    CorpseVariantName = "",
    Name = "green slime",
    Species = Species.Ooze,
    MovementRate = 6,
    Size = Size.Medium,
    Weight = 400,
    Nutrition = 150,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "DecayResistance",
        "Breathlessness",
        "Amorphism",
        "NonAnimal",
        "Eyelessness",
        "Limblessness",
        "Headlessness",
        "Mindlessness",
        "Asexuality",
        "Omnivorism"
    }
,
    ValuedProperties = new Dictionary<string, Object>
    {
        {
            "ColdResistance",
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
            "AcidResistance",
            3
        },
        {
            "StoningResistance",
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
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Touch, Timeout = 1, Effects = new AbilityEffect[] { new Slime() } },
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new Slime() } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Slime() } }
    }
}
