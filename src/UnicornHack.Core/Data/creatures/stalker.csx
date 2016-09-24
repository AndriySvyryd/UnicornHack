new CreatureVariant
{
    InitialLevel = 8,
    ArmorClass = 3,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.Wandering | MonsterBehavior.Stalking,
    Name = "stalker",
    Species = Species.Elemental,
    SpeciesClass = SpeciesClass.Extraplanar,
    MovementRate = 12,
    Size = Size.Large,
    Weight = 900,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Invisibility", "InvisibilityDetection", "Infravision", "AnimalBody" },
    ValuedProperties = new Dictionary<string, Object> { { "Stealthiness", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
    }
}
