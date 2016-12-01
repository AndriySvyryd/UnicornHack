new CreatureVariant
{
    Name = "iron golem",
    Species = Species.Golem,
    CorpseVariantName = "",
    InitialLevel = 18,
    ArmorClass = 3,
    MagicResistance = 60,
    MovementRate = 6,
    Weight = 2000,
    Size = Size.Large,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 22 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Breath,
            Timeout = 5,
            Effects = new AbilityEffect[] { new PoisonDamage { Damage = 14 } }
        }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "NonAnimal",
        "Breathlessness",
        "Mindlessness",
        "Humanoidness",
        "Asexuality",
        "StoningResistance",
        "SlimingResistance",
        "SicknessResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object>
    {
        {
            "WaterWeakness",
            3
        },
        {
            "ColdResistance",
            3
        },
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
            "ThickHide",
            3
        },
        {
            "MaxHP",
            80
        }
    }
,
    GenerationFrequency = Frequency.Rarely
}
