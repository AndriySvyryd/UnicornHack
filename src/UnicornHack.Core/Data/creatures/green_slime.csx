new CreatureVariant
{
    Name = "green slime",
    Species = Species.Ooze,
    CorpseVariantName = "",
    InitialLevel = 6,
    ArmorClass = 6,
    MovementRate = 6,
    Weight = 400,
    Size = Size.Medium,
    Nutrition = 150,
    Abilities = new List<Ability>
    {
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Touch, Timeout = 1, Effects = new AbilityEffect[] { new Slime() } },
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new Slime() } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Slime() } }
    }
,
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
        "Omnivorism",
        "StoningResistance"
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
            "Stealthiness",
            3
        }
    }
,
    GenerationFlags = GenerationFlags.HellOnly,
    GenerationFrequency = Frequency.Rarely
}
