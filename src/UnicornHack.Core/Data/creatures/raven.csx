new CreatureVariant
{
    InitialLevel = 4,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Squawk,
    Name = "raven",
    Species = Species.Crow,
    SpeciesClass = SpeciesClass.Bird,
    MovementRate = 20,
    Size = Size.Tiny,
    Weight = 100,
    Nutrition = 40,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravisibility", "AnimalBody", "Handlessness", "Oviparity", "Omnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
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
            Effects = new AbilityEffect[] { new Blind { Duration = 13 } }
        }
    }
}
