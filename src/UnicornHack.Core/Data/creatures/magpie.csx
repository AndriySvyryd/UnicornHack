new CreatureVariant
{
    Name = "magpie",
    Species = Species.Crow,
    SpeciesClass = SpeciesClass.Bird,
    InitialLevel = 2,
    ArmorClass = 6,
    MovementRate = 20,
    Weight = 50,
    Size = Size.Tiny,
    Nutrition = 20,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravisibility", "AnimalBody", "Handlessness", "Oviparity", "Omnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.Wandering | MonsterBehavior.GemCollector,
    Noise = ActorNoiseType.Squawk
}
