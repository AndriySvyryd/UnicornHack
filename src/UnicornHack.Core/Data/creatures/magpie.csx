new Creature
{
    Name = "magpie",
    Species = Species.Crow,
    SpeciesClass = SpeciesClass.Bird,
    ArmorClass = 6,
    MovementRate = 20,
    Weight = 50,
    Size = Size.Tiny,
    Nutrition = 20,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravisibility", "AnimalBody", "Handlessness", "Oviparity", "Omnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    InitialLevel = 2,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.Wandering | MonsterBehavior.GemCollector,
    Noise = ActorNoiseType.Squawk
}
