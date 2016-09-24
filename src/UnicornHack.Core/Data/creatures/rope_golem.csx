new CreatureVariant
{
    InitialLevel = 4,
    ArmorClass = 8,
    GenerationFrequency = Frequency.Rarely,
    CorpseVariantName = "",
    Name = "rope golem",
    Species = Species.Golem,
    MovementRate = 12,
    Size = Size.Large,
    Weight = 450,
    SimpleProperties = new HashSet<string> { "SleepResistance", "NonAnimal", "Breathlessness", "Mindlessness", "Humanoidness", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object>
    {
        {
            "ColdResistance",
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
            "SicknessResistance",
            3
        },
        {
            "MaxHP",
            30
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Hug,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
    }
}
