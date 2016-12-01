new CreatureVariant
{
    Name = "black naga",
    Species = Species.Naga,
    SpeciesClass = SpeciesClass.Aberration,
    PreviousStageName = "black naga hatchling",
    InitialLevel = 8,
    ArmorClass = 2,
    MagicResistance = 10,
    MovementRate = 14,
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Spit,
            Timeout = 1,
            Effects = new AbilityEffect[] { new AcidDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Infravision", "SerpentlikeBody", "Limblessness", "Carnivorism", "Oviparity", "SingularInventory", "StoningResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "AcidResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "ThickHide", 3 } },
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = 4,
    Noise = ActorNoiseType.Hiss
}
