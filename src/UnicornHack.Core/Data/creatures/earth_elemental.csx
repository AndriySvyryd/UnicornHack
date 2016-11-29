new CreatureVariant
{
    InitialLevel = 8,
    ArmorClass = 2,
    MagicResistance = 30,
    GenerationFrequency = Frequency.Uncommonly,
    CorpseVariantName = "",
    Name = "earth elemental",
    Species = Species.Elemental,
    SpeciesClass = SpeciesClass.Extraplanar,
    MovementRate = 6,
    Size = Size.Huge,
    Weight = 2500,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Phasing",
        "NonAnimal",
        "Breathlessness",
        "Limblessness",
        "Eyelessness",
        "Headlessness",
        "Mindlessness",
        "Asexuality"
    }
,
    ValuedProperties = new Dictionary<string, Object>
    {
        {
            "FireResistance",
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
            "StoningResistance",
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
            "ThickHide",
            3
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 14 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Stun { Duration = 2 } }
        }
    }
}
