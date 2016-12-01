new CreatureVariant
{
    Name = "wood golem",
    Species = Species.Golem,
    CorpseVariantName = "",
    InitialLevel = 7,
    ArmorClass = 4,
    MovementRate = 3,
    Weight = 1000,
    Size = Size.Large,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "NonAnimal", "Breathlessness", "Mindlessness", "Humanoidness", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "ThickHide", 3 }, { "MaxHP", 50 } },
    GenerationFrequency = Frequency.Rarely
}
