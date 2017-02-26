new Creature
{
    Name = "bat",
    Species = Species.Bat,
    SpeciesClass = SpeciesClass.Bird,
    ArmorClass = 8,
    MovementDelay = 54,
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
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravisibility", "AnimalBody", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Stealthiness", 3 } },
    InitialLevel = 1,
    NextStageName = "giant bat",
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Sqeek
}
