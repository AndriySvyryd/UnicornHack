new CreatureVariant
{
    InitialLevel = 7,
    ArmorClass = 4,
    GenerationFrequency = Frequency.Rarely,
    CorpseVariantName = "",
    Name = "wood golem",
    Species = Species.Golem,
    MovementRate = 3,
    Size = Size.Large,
    Weight = 1000,
    SimpleProperties = new HashSet<string> { "SleepResistance", "NonAnimal", "Breathlessness", "Mindlessness", "Humanoidness", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "ThickHide", 3 }, { "MaxHP", 50 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
    }
}
