new CreatureVariant
{
    InitialLevel = 18,
    ArmorClass = 3,
    MagicResistance = 60,
    GenerationFrequency = Frequency.Rarely,
    CorpseVariantName = "",
    Name = "iron golem",
    Species = Species.Golem,
    MovementRate = 6,
    Size = Size.Large,
    Weight = 2000,
    SimpleProperties = new HashSet<string> { "SleepResistance", "NonAnimal", "Breathlessness", "Mindlessness", "Humanoidness", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object>
    {
        {
            "WaterResistance",
            -3
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
            80
        }
    }
,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 22 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Breath,
            Timeout = 5,
            Effects = new AbilityEffect[] { new PoisonDamage { Damage = 14 } }
        }
    }
}
