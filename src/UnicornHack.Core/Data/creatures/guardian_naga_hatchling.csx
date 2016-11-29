new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Uncommonly,
    Noise = ActorNoiseType.Hiss,
    NextStageName = "guardian naga",
    Name = "guardian naga hatchling",
    Species = Species.Naga,
    SpeciesClass = SpeciesClass.Aberration,
    MovementRate = 10,
    Size = Size.Medium,
    Weight = 500,
    Nutrition = 200,
    SimpleProperties = new HashSet<string> { "InvisibilityDetection", "Infravision", "SerpentlikeBody", "Limblessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "ThickHide", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
    }
}
