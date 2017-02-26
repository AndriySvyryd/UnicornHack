new Creature
{
    Name = "acid blob",
    Species = Species.Blob,
    ArmorClass = 8,
    MovementDelay = 400,
    Weight = 30,
    Size = Size.Tiny,
    Nutrition = 1,
    Abilities = new HashSet<Ability>
    {
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new HashSet<Effect> { new AcidDamage { Damage = 4 } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new AcidDamage { Damage = 4 } } }
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
    InitialLevel = 1,
    GenerationFrequency = Frequency.Usually,
    Behavior = MonsterBehavior.Wandering
}
