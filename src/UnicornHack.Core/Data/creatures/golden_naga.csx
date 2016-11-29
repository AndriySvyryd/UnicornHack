new CreatureVariant
{
    InitialLevel = 10,
    ArmorClass = 2,
    MagicResistance = 70,
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = 5,
    Noise = ActorNoiseType.Hiss,
    PreviousStageName = "golden naga hatchling",
    Name = "golden naga",
    Species = Species.Naga,
    SpeciesClass = SpeciesClass.Aberration,
    MovementRate = 14,
    Size = Size.Huge,
    Weight = 1500,
    Nutrition = 600,
    SimpleProperties = new HashSet<string> { "Infravision", "SerpentlikeBody", "Limblessness", "Carnivorism", "Oviparity", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "AcidResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "StoningResistance", 3 }, { "ThickHide", 3 } },
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
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new AbilityEffect[] { new MagicalDamage { Damage = 14 } }
        }
    }
}
