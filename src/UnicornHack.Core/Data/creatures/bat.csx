new CreatureVariant
{
    InitialLevel = 1,
    ArmorClass = 8,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Sqeek,
    NextStageName = "giant bat",
    Name = "bat",
    Species = Species.Bat,
    SpeciesClass = SpeciesClass.Bird,
    MovementRate = 22,
    Size = Size.Tiny,
    Weight = 50,
    Nutrition = 20,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravisibility", "AnimalBody", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Stealthiness", 3 } },
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
