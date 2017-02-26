new Creature
{
    Name = "wood golem",
    Species = Species.Golem,
    ArmorClass = 4,
    MovementDelay = 400,
    Weight = 1000,
    Size = Size.Large,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "NonAnimal", "Breathlessness", "Mindlessness", "Humanoidness", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "ThickHide", 3 }, { "MaxHP", 50 } },
    InitialLevel = 7,
    CorpseName = "",
    GenerationFrequency = Frequency.Rarely
}
