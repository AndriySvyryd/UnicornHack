new CreatureVariant
{
    Name = "guardian naga",
    Species = Species.Naga,
    SpeciesClass = SpeciesClass.Aberration,
    PreviousStageName = "guardian naga hatchling",
    InitialLevel = 12,
    MagicResistance = 50,
    MovementRate = 16,
    Weight = 1500,
    Size = Size.Huge,
    Nutrition = 600,
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
,
    SimpleProperties = new HashSet<string> { "Infravision", "SerpentlikeBody", "Limblessness", "Carnivorism", "Oviparity", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "ThickHide", 3 } },
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = 7,
    Noise = ActorNoiseType.Hiss
}
