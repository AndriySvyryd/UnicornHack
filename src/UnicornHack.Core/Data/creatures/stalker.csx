new CreatureVariant
{
    Name = "stalker",
    Species = Species.Elemental,
    SpeciesClass = SpeciesClass.Extraplanar,
    InitialLevel = 8,
    ArmorClass = 3,
    MovementRate = 12,
    Weight = 900,
    Size = Size.Large,
    Nutrition = 400,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Invisibility", "InvisibilityDetection", "Infravision", "AnimalBody" },
    ValuedProperties = new Dictionary<string, Object> { { "Stealthiness", 3 } },
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.Wandering | MonsterBehavior.Stalking
}
