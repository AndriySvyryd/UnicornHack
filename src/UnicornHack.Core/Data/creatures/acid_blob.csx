new CreatureVariant
{
    InitialLevel = 1,
    ArmorClass = 8,
    GenerationFrequency = Frequency.Usually,
    Behavior = MonsterBehavior.Wandering,
    Name = "acid blob",
    Species = Species.Blob,
    MovementRate = 3,
    Size = Size.Tiny,
    Weight = 30,
    Nutrition = 1,
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
        "Metallivorism"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "AcidResistance", 3 }, { "StoningResistance", 3 }, { "Stealthiness", 3 } },
    Abilities = new List<Ability>
    {
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new AcidDamage { Damage = 4 } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new AcidDamage { Damage = 4 } } }
    }
}
