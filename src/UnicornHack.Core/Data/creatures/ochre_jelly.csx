new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 8,
    MagicResistance = 20,
    GenerationFrequency = Frequency.Commonly,
    Name = "ochre jelly",
    Species = Species.Jelly,
    MovementRate = 3,
    Size = Size.Small,
    Weight = 100,
    Nutrition = 20,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Breathlessness",
        "Amorphism",
        "NonAnimal",
        "Eyelessness",
        "Limblessness",
        "Headlessness",
        "Mindlessness",
        "Asexuality",
        "NoInventory"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "AcidResistance", 3 }, { "StoningResistance", 3 }, { "Stealthiness", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Engulf { Duration = 4 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnDigestion,
            Action = AbilityAction.Digestion,
            Timeout = 1,
            Effects = new AbilityEffect[] { new AcidDamage { Damage = 10 } }
        }
,
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new AcidDamage { Damage = 7 } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new AcidDamage { Damage = 7 } } }
    }
}
