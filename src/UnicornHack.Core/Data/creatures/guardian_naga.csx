new CreatureVariant
{
    InitialLevel = 12,
    MagicResistance = 50,
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = 7,
    Noise = ActorNoiseType.Hiss,
    PreviousStageName = "guardian naga hatchling",
    Name = "guardian naga",
    Species = Species.Naga,
    SpeciesClass = SpeciesClass.Aberration,
    MovementRate = 16,
    Size = Size.Huge,
    Weight = 1500,
    Nutrition = 600,
    SimpleProperties = new HashSet<string> { "Infravision", "SerpentlikeBody", "Limblessness", "Carnivorism", "Oviparity", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "ThickHide", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Spit,
            Timeout = 1,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 14 } }
        }
    }
}
