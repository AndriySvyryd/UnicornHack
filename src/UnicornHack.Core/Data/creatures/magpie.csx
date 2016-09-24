new CreatureVariant
{
    InitialLevel = 2,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.Wandering | MonsterBehavior.GemCollector,
    Noise = ActorNoiseType.Squawk,
    Name = "magpie",
    Species = Species.Crow,
    SpeciesClass = SpeciesClass.Bird,
    MovementRate = 20,
    Size = Size.Tiny,
    Weight = 50,
    Nutrition = 20,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravisibility", "AnimalBody", "Handlessness", "Oviparity", "Omnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
    }
}
