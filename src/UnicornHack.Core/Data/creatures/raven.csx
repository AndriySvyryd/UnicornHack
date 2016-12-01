new CreatureVariant
{
    Name = "raven",
    Species = Species.Crow,
    SpeciesClass = SpeciesClass.Bird,
    InitialLevel = 4,
    ArmorClass = 6,
    MovementRate = 20,
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
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Blind { Duration = 13 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravisibility", "AnimalBody", "Handlessness", "Oviparity", "Omnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Squawk
}
