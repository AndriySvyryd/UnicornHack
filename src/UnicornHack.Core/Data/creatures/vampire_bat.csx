new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Sqeek,
    PreviousStageName = "giant bat",
    Name = "vampire bat",
    Species = Species.Bat,
    SpeciesClass = SpeciesClass.Bird,
    MovementRate = 20,
    Size = Size.Tiny,
    Weight = 100,
    Nutrition = 40,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravisibility", "AnimalBody", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Regeneration", 3 }, { "Stealthiness", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new DrainStrength { Amount = 1 } }
        }
    }
}
