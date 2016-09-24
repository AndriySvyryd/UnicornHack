new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 7,
    GenerationFrequency = Frequency.Uncommonly,
    Name = "violet fungus",
    Species = Species.Fungus,
    MovementRate = 1,
    Size = Size.Small,
    Weight = 100,
    Nutrition = 100,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Breathlessness",
        "NonAnimal",
        "Eyelessness",
        "Limblessness",
        "Headlessness",
        "Mindlessness",
        "Asexuality",
        "NoInventory"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "Stealthiness", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PoisonDamage { Damage = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.Targetted, Action = AbilityAction.Touch, Timeout = 1, Effects = new AbilityEffect[] { new Stick() } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 3 } } }
    }
}
