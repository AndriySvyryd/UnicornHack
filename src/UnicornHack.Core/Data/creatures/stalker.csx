new Creature
{
    Name = "stalker",
    Species = Species.Elemental,
    SpeciesClass = SpeciesClass.Extraplanar,
    ArmorClass = 3,
    MovementDelay = 100,
    Weight = 900,
    Size = Size.Large,
    Nutrition = 400,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 10 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Invisibility", "InvisibilityDetection", "Infravision", "AnimalBody" },
    ValuedProperties = new Dictionary<string, Object> { { "Stealthiness", 3 } },
    InitialLevel = 8,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.Wandering | MonsterBehavior.Stalking
}
