new Creature
{
    Name = "giant bat",
    Species = Species.Bat,
    SpeciesClass = SpeciesClass.Bird,
    ArmorClass = 7,
    MovementRate = 22,
    Weight = 100,
    Size = Size.Tiny,
    Nutrition = 40,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravisibility", "AnimalBody", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Stealthiness", 3 } },
    InitialLevel = 2,
    PreviousStageName = "bat",
    NextStageName = "vampire bat",
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Sqeek
}
