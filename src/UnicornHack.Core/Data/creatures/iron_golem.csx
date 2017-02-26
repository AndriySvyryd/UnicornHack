new Creature
{
    Name = "iron golem",
    Species = Species.Golem,
    ArmorClass = 3,
    MagicResistance = 60,
    MovementDelay = 200,
    Weight = 2000,
    Size = Size.Large,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 22 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Breath,
            Timeout = 5,
            Effects = new HashSet<Effect> { new PoisonDamage { Damage = 14 } }
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
    InitialLevel = 18,
    CorpseName = "",
    GenerationFrequency = Frequency.Rarely
}
