new CreatureVariant
{
    Name = "acid blob",
    Species = Species.Blob,
    InitialLevel = 1,
    ArmorClass = 8,
    MovementRate = 3,
    Weight = 30,
    Size = Size.Tiny,
    Nutrition = 1,
    Abilities = new List<Ability>
    {
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new AcidDamage { Damage = 4 } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new AcidDamage { Damage = 4 } } }
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
        "Metallivorism",
        "StoningResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "AcidResistance", 3 }, { "Stealthiness", 3 } },
    GenerationFrequency = Frequency.Usually,
    Behavior = MonsterBehavior.Wandering
}
