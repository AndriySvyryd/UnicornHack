new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 8,
    GenerationFrequency = Frequency.Rarely,
    Name = "brown pudding",
    Species = Species.Pudding,
    MovementRate = 3,
    Size = Size.Medium,
    Weight = 512,
    Nutrition = 256,
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
        "Reanimation"
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
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new VenomDamage { Damage = 3 } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new VenomDamage { Damage = 3 } } }
    }
}
