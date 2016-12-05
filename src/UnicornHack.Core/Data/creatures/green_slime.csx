new Creature
{
    Name = "green slime",
    Species = Species.Ooze,
    ArmorClass = 6,
    MovementRate = 6,
    Weight = 400,
    Size = Size.Medium,
    Nutrition = 150,
    Abilities = new HashSet<Ability>
    {
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Touch, Timeout = 1, Effects = new HashSet<Effect> { new Slime { } } },
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new HashSet<Effect> { new Slime { } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new Slime { } } }
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
    InitialLevel = 6,
    CorpseName = "",
    GenerationFlags = GenerationFlags.HellOnly,
    GenerationFrequency = Frequency.Rarely
}
