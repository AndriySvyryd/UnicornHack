new Creature
{
    Name = "raven",
    Species = Species.Crow,
    SpeciesClass = SpeciesClass.Bird,
    ArmorClass = 6,
    MovementRate = 20,
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
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new Blind { Duration = 13 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravisibility", "AnimalBody", "Handlessness", "Oviparity", "Omnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    InitialLevel = 4,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Squawk
}
