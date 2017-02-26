new Creature
{
    Name = "guardian naga",
    Species = Species.Naga,
    SpeciesClass = SpeciesClass.Aberration,
    MagicResistance = 50,
    MovementDelay = 75,
    Weight = 1500,
    Size = Size.Huge,
    Nutrition = 600,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Spit,
            Timeout = 1,
            Effects = new HashSet<Effect> { new VenomDamage { Damage = 14 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Infravision", "SerpentlikeBody", "Limblessness", "Carnivorism", "Oviparity", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "ThickHide", 3 } },
    InitialLevel = 12,
    PreviousStageName = "guardian naga hatchling",
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = 7,
    Noise = ActorNoiseType.Hiss
}
