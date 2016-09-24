new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 8,
    GenerationFrequency = Frequency.Usually,
    Behavior = MonsterBehavior.Wandering,
    Name = "quivering blob",
    Species = Species.Blob,
    MovementRate = 1,
    Size = Size.Small,
    Weight = 200,
    Nutrition = 100,
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
        "Asexuality"
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 4 } }
        }
    }
}
