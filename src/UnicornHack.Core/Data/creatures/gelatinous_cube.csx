new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 8,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.Wandering | MonsterBehavior.WeaponCollector,
    Name = "gelatinous cube",
    Species = Species.Blob,
    MovementRate = 6,
    Size = Size.Large,
    Weight = 600,
    Nutrition = 150,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "DecayResistance",
        "Breathlessness",
        "NonAnimal",
        "Eyelessness",
        "Limblessness",
        "Headlessness",
        "Mindlessness",
        "Omnivorism",
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
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Paralyze { Duration = 4 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 7,
            Effects = new AbilityEffect[] { new Engulf { Duration = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnDigestion,
            Action = AbilityAction.Digestion,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PoisonDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnDigestion,
            Action = AbilityAction.Digestion,
            Timeout = 1,
            Effects = new AbilityEffect[] { new AcidDamage { Damage = 1 } }
        }
,
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new Paralyze { Duration = 4 } } }
    }
}
