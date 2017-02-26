new Creature
{
    Name = "brown pudding",
    Species = Species.Pudding,
    ArmorClass = 8,
    MovementDelay = 400,
    Weight = 512,
    Size = Size.Medium,
    Nutrition = 256,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new VenomDamage { Damage = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new HashSet<Effect> { new VenomDamage { Damage = 3 } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new VenomDamage { Damage = 3 } } }
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
        "Reanimation",
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
    InitialLevel = 5,
    GenerationFrequency = Frequency.Rarely
}
