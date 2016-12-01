new CreatureVariant
{
    Name = "giant bat",
    Species = Species.Bat,
    SpeciesClass = SpeciesClass.Bird,
    PreviousStageName = "bat",
    NextStageName = "vampire bat",
    InitialLevel = 2,
    ArmorClass = 7,
    MovementRate = 22,
    Weight = 100,
    Size = Size.Tiny,
    Nutrition = 40,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravisibility", "AnimalBody", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Stealthiness", 3 } },
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Sqeek
}
