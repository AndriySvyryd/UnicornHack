new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 4,
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = -4,
    Noise = ActorNoiseType.Hiss,
    PreviousStageName = "red naga hatchling",
    Name = "red naga",
    Species = Species.Naga,
    SpeciesClass = SpeciesClass.Aberration,
    MovementRate = 12,
    Size = Size.Huge,
    Weight = 1500,
    Nutrition = 600,
    SimpleProperties = new HashSet<string> { "Infravision", "SerpentlikeBody", "Limblessness", "Carnivorism", "Oviparity", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "SlimingResistance", 3 }, { "ThickHide", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Spit,
            Timeout = 1,
            Effects = new AbilityEffect[] { new FireDamage { Damage = 7 } }
        }
    }
}
