new Creature
{
    Name = "black naga",
    Species = Species.Naga,
    SpeciesClass = SpeciesClass.Aberration,
    ArmorClass = 2,
    MagicResistance = 10,
    MovementDelay = 85,
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
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Spit,
            Timeout = 1,
            Effects = new HashSet<Effect> { new AcidDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Infravision", "SerpentlikeBody", "Limblessness", "Carnivorism", "Oviparity", "SingularInventory", "StoningResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "AcidResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "ThickHide", 3 } },
    InitialLevel = 8,
    PreviousStageName = "black naga hatchling",
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = 4,
    Noise = ActorNoiseType.Hiss
}
